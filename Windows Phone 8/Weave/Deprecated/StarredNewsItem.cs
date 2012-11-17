using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using ProtoBuf;

namespace weave
{
    [DataContract]
    [ProtoContract]
    public class StarredNewsItem : INewsItem
    {
        [DataMember][ProtoMember(1)] public string Title { get; set; }
        [DataMember][ProtoMember(2)] public string Link { get; set; }
        [DataMember][ProtoMember(3)] public string Description { get; set; }
        [DataMember][ProtoMember(4)] public DateTime PublishDateTime { get; set; }
        [DataMember][ProtoMember(5)] public string ImageUrl { get; set; }
        [DataMember][ProtoMember(6)] public string FeedName { get; set; }
        [DataMember][ProtoMember(7)] public string Category { get; set; }
        [DataMember][ProtoMember(8)] public string FullArticleHtml { get; set; }

        object syncObject = new object();
        IObservable<string> fullArticleHtmlStream;
        Subject<Unit> notifier = new Subject<Unit>();

        public IObservable<string> FullArticleHtmlStream
        {
            get
            {
                lock (syncObject)
                {
                    if (FullArticleHtml == null)
                        return notifier.Take(1).Select(notUsed => FullArticleHtml);
                    else
                        return new[] { FullArticleHtml }.ToObservable().Take(1);
                }
            }
        }

       // public IObservable<string> FullArticleHtmlStream { get; private set; }

        public void CheckIfFullArticleIsPresent()
        {
            if (string.IsNullOrEmpty(FullArticleHtml))
            {
                var uri = Link.ToUri();
                if (uri == null)
                {
                    MessageBox.Show("This article has a bad link - we can't show it!");
                    return;
                }
                var request = uri.ToWebRequest() as HttpWebRequest;
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0)";
                fullArticleHtmlStream = request
                    .GetHttpWebResponseAsync()
                    .Do(notUsed => Thread.Sleep(15000))
                    .Where(response => response.StatusCode == HttpStatusCode.OK)
                    .Select(response =>
                    {
                        var responseString = response.GetResponseStreamAsString();
                        response.Close();
                        response.Dispose();
                        return responseString;
                    });

                fullArticleHtmlStream
                    .Subscribe(o =>
                    {
                        lock (syncObject)
                        {
                            FullArticleHtml = o;
                        }
                        notifier.OnNext(new Unit());
                    });
                    //.Publish();
                //fullArticleHtmlStream.Subscribe(o => FullArticleHtml = o);
                    
                //fullArticleHtmlStream.Subscribe(o => FullArticleHtml = o);
            }
        }

        public string PublishDate
        {
            get { return PublishDateTime.ElapsedTime(); }
        }

        public string FormattedForPopupsSourceAndDate
        {
            get
            {
                return PublishDateTime.ToString("MMMM dd, h:mm tt") +
                    " via " + FeedName;
            }
        }

        public string FormattedForMainPageSourceAndDate
        {
            get
            {
                return PublishDateTime.ElapsedTime() + " via " + FeedName;
            }
        }

        public bool HasImage
        {
            get
            {
                return !string.IsNullOrEmpty(ImageUrl) &&
                    Uri.IsWellFormedUriString(ImageUrl, UriKind.Absolute);
            }
        }

        public override string ToString()
        {
            //return string.Format("{0}: {1}   from {2}", Category, Title, OriginalFeedUri);
            return string.Format("{0}: {1}", Category, Title);
        }

        //public void SanitizeDescription()
        //{
        //    Description = HttpUtility.HtmlDecode(RssServiceLayer.Sanitize(Description).Trim());
        //}
    }
}
