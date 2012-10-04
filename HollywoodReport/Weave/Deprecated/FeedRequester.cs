using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace weave.Services.RSS
{
    internal class FeedRequester
    {
        //public TimeSpan TimeOut { get; set; }
        public string FeedUri { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; private set; }
        public string Etag { get; set; }
        //public string LastModified { get; set; }
        public List<NewsItem> News { get; private set; }
        public RequestStatus Status { get; private set; }

        public enum RequestStatus
        {
            OK,
            Unmodified
        }


        public FeedRequester()
        {
            this.News = new List<NewsItem>();
            //this.TimeOut = TimeSpan.FromMinutes(1);
        }


        public IObservable<RequestStatus> UpdateFeed()
        {
            return Observable.Create<RequestStatus>(observer =>
            {
                var disposable = Disposable.Empty;
                try
                {
                    DebugEx.WriteLine(string.Format("CLIENT SIDE Calling: {0}", this.FeedUri));

                    var request = this.FeedUri.ToUri().ToWebRequest() as HttpWebRequest;


                    #region CONDITIONAL GET

                    if (!string.IsNullOrEmpty(Etag))
                    {
                        request.Headers[HttpRequestHeader.IfNoneMatch] = Etag;
                    }

                    //if (!string.IsNullOrEmpty(LastModified))
                    //{
                    //    DateTime lastModified;
                    //    if (DateTime.TryParse(LastModified, out lastModified))
                    //        request.IfModifiedSince = lastModified;
                    //}

                    #endregion

                   
                    disposable = FeedWebRequestServiceBus.Instance.Enqueue(request)
                        .Take(1)
                        .Subscribe(o =>
                        {
                            try
                            {
                                if (o.StatusCode == HttpStatusCode.NotModified)
                                {
                                    HandleNonModifiedResponse();
                                    observer.OnNext(this.Status);
                                    observer.OnCompleted();
                                }

                                else if (o.StatusCode == HttpStatusCode.OK)
                                {
                                    HandleNewData(o);
                                    observer.OnNext(this.Status);
                                    observer.OnCompleted();
                                }

                                else
                                {
                                    observer.OnError(new Exception(o.StatusCode.ToString()));
                                }
                            }
                            catch (Exception ex)
                            {
                                DebugEx.WriteLine(string.Format("CLIENT SIDE {0} call failed: {1}", this.FeedUri, ex.ToString()));
                                observer.OnError(ex);
                            }
                            finally
                            {
                                o.Close();
                                ((IDisposable)o).Dispose();
                            }
                        },
                        ex =>
                        {
                            DebugEx.WriteLine(string.Format("CLIENT SIDE {0} call failed: {1}", this.FeedUri, ex.ToString()));
                            observer.OnError(ex);
                        });
                }
                catch (Exception ex)
                {
                    DebugEx.WriteLine("CLIENT SIDE Exception calling {0}: FeedSource.CheckForAndGetNewFeedData\r\n{1}",
                        FeedUri,
                        ex);
                    observer.OnError(ex);
                }
                return disposable;
            });
        }

        void HandleNonModifiedResponse()
        {
            DebugEx.WriteLine(string.Format("CLIENT SIDE ### Using cached data for {0}", this.FeedUri));
            this.Status = RequestStatus.Unmodified;
        }

        void HandleNewData(HttpWebResponse o)
        {
            DebugEx.WriteLine(string.Format("CLIENT SIDE Refreshing {0} with new data", this.FeedUri));

            #region CONDITIONAL GET

            var eTag = o.Headers["ETag"];
            if (!string.IsNullOrEmpty(eTag))
                this.Etag = eTag;

            //var lastModified = o.Headers[HttpResponseHeader.LastModified];
            //if (!string.IsNullOrEmpty(lastModified))
            //    this.LastModified = lastModified;

            #endregion

            using (var stream = o.GetResponseStream())
            {
                ParseNewsFromLastRefreshTime(stream);
                stream.Close();
            }
        }

        void ParseNewsFromLastRefreshTime(Stream stream)
        {
            var elementsWithDate = stream
                .ToXElements()
                .ToXElementsWithLocalDate()
                .OrderByDescending(o => o.Item1);


            var previousMostRecentNewsItemPubDateString = this.MostRecentNewsItemPubDate;


            var mostRecentItem = elementsWithDate.FirstOrDefault();
            if (mostRecentItem != null)
                this.MostRecentNewsItemPubDate = mostRecentItem.Item2.Element("pubDate").ValueOrDefault();

            var oldestItem = elementsWithDate.LastOrDefault();
            if (oldestItem != null)
                this.OldestNewsItemPubDate = oldestItem.Item2.Element("pubDate").ValueOrDefault();


            IEnumerable<Tuple<DateTime, XElement>> filteredNews = elementsWithDate;

            var tryGetPreviousMostRecentDate = RssServiceLayer.TryGetLocalDate(previousMostRecentNewsItemPubDateString);
            if (tryGetPreviousMostRecentDate.Item1)
            {
                var previousMostRecentNewsItemPubDate = tryGetPreviousMostRecentDate.Item2;
                filteredNews = elementsWithDate.TakeWhile(o => o.Item1 > previousMostRecentNewsItemPubDate);
            }

            var news = filteredNews
                .Select(o => o.Item2.ToNewsItem(o.Item1))
                .OfType<NewsItem>()
                .ToList();

            this.News = news;

            this.Status = RequestStatus.OK;
        }
    }
}
