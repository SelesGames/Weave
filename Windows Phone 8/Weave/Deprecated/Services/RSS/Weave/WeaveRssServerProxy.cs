using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using ICSharpCode.SharpZipLib.GZip;
using ProtoBuf;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace weave.Services.RSS.Cloud
{
    public class WeaveRssServerProxy : IDisposable
    {
        //const string SERVICE_URL = "http://192.168.1.72:8086/Weave";
        //const string PING_URL = "http://192.168.1.72:8086/Ping/";

        const string SERVICE_URL = "http://weave.cloudapp.net:8086/Weave";
        const string PING_URL = "http://weave.cloudapp.net:8086/Ping/";

        List<FeedRequest> outgoingFeedRequests;
        CompositeDisposable disposables = new CompositeDisposable();
        HttpWebRequest request;
        IObserver<IEnumerable<FeedResult>> observer;
        static readonly TimeSpan pingTimeout = TimeSpan.FromSeconds(4);

        public static IObservable<IEnumerable<FeedResult>> GetFeedResultsAsync(List<FeedRequest> outgoingFeedRequests)
        {
            return Observable.CreateWithDisposable<IEnumerable<FeedResult>>(observer =>
            {
                var request = new WeaveRssServerProxy(outgoingFeedRequests, observer);
                return request;
            });
        }



        #region Ping the server to see if it is up and running (and responsive)

        public static IObservable<bool> PingServerAsync()
        {
            return Observable.CreateWithDisposable<bool>(observer =>
            {
                var disp = Disposable.Empty;
                try
                {
                    disp = PING_URL.ToUri().ToWebRequest().GetWebResponseAsyncWithTimeout(pingTimeout).Subscribe(
                        o =>
                        {
                            try
                            {
                                var response = o as HttpWebResponse;
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    observer.OnNext(true);
                                    observer.OnCompleted();
                                }
                                else
                                {
                                    observer.OnNext(false);
                                    observer.OnCompleted();
                                }
                            }
                            catch (Exception)
                            {
                                observer.OnNext(false);
                                observer.OnCompleted();
                            }
                        },
                        exception =>
                        {
                            observer.OnNext(false);
                            observer.OnCompleted();
                        });
                }
                catch(Exception)
                {
                    observer.OnNext(false);
                    observer.OnCompleted();
                }
                return disp;
            });
        }

        #endregion



        private WeaveRssServerProxy(
            List<FeedRequest> outgoingFeedRequests,
            IObserver<IEnumerable<FeedResult>> observer)
        {
            this.outgoingFeedRequests = outgoingFeedRequests;
            this.observer = observer;

            try
            {
                request = SERVICE_URL.ToUri().ToWebRequest() as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/json";
                GetLatestNews();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        void GetLatestNews()
        {
            var writeSerializer = new DataContractJsonSerializer(typeof(List<FeedRequest>));
            Action<Stream, object> serializer = writeSerializer.WriteObject;
            //Action<Stream, object> serializer = Serializer.Serialize;

            request.WriteToRequestStreamAsync(outgoingFeedRequests, serializer)
                .Subscribe(notUsed => CallRequest(), HandleException)
                .DisposeWith(disposables);
        }

        void CallRequest()
        {
            FeedWebRequestServiceBus.Instance.Enqueue(request)
                .Subscribe(
                    response =>
                    {
                        HandleResponse(response);
                    }, 
                    exception => 
                    {
                        HandleException(exception);
                    })
                .DisposeWith(disposables);
        }

        void HandleResponse(WebResponse response)
        {
            try
            {
                var results = ParseFeedResultsFromWebResponse(response);
                observer.OnNext(results);
                observer.OnCompleted();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        void HandleException(Exception exception)
        {
            observer.OnError(exception);
        }



        #region Helper Methods

        IEnumerable<FeedResult> ParseFeedResultsFromWebResponse(WebResponse response)
        {
            var sw = Stopwatch.StartNew();
            using (var stream = response.GetResponseStream())
            using (var gzip = new GZipInputStream(stream))
            {
                var feedResults = Serializer.Deserialize<List<FeedResult>>(gzip);
                gzip.Close();
                stream.Close();
                sw.Stop();
                DebugEx.WriteLine("Took {0} ms to deserialize", sw.ElapsedMilliseconds);
                return feedResults;
            }
        }

        #endregion



        public void Dispose()
        {
            disposables.Dispose();
            try
            {
                if (request != null)
                    request.Abort();
            }
            catch (Exception) {}
        }
    }
}