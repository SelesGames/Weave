using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ProtoBuf;
using Weave.RSS.DTOs.Outgoing;
using Weave.RSS.DTOs.Incoming;

namespace weave.Services.RSS
{
    internal class WeaveRssServerProxy : IDisposable
    {
        /// LOCAL
        /// ****************************
        //const string SERVICE_URL = "http://localhost:8086/Weave";
        //const string PING_URL = "http://localhost:8086/Ping/";


        /// STAGING
        /// ****************************
        //const string SERVICE_URL = "http://5c6ea5aab3a74d64ad1ba9bdb3718f13.cloudapp.net:8086/Weave";
        //const string PING_URL = "http://5c6ea5aab3a74d64ad1ba9bdb3718f13.cloudapp.net:8086/Ping/";


        /// PRODUCTION
        /// ****************************
        const string SERVICE_URL = "http://weave.cloudapp.net:8086/Weave?fsd=true";
        const string PING_URL = "http://weave.cloudapp.net:8086/Ping/";

        List<FeedRequest> outgoingFeedRequests;
        HttpWebRequest request;
        //IObserver<IEnumerable<FeedResult>> observer;
        static readonly TimeSpan pingTimeout = TimeSpan.FromSeconds(4);

        //public static IObservable<IEnumerable<FeedResult>> GetFeedResultsAsync(List<FeedRequest> outgoingFeedRequests)
        //{
        //    return Observable.Create<IEnumerable<FeedResult>>(observer =>
        //    {
        //        var request = new WeaveRssServerProxy(outgoingFeedRequests, observer);
        //        return request;
        //    });
        //}




        #region Ping the server to see if it is up and running (and responsive)

        //public static IObservable<bool> PingServerAsync()
        //{
        //    return Observable.Create<bool>(observer =>
        //    {
        //        var disp = Disposable.Empty;
        //        try
        //        {
        //            disp = PING_URL.ToUri().ToWebRequest().GetWebResponseAsyncWithTimeout(pingTimeout).Subscribe(
        //                o =>
        //                {
        //                    try
        //                    {
        //                        var response = o as HttpWebResponse;
        //                        if (response.StatusCode == HttpStatusCode.OK)
        //                        {
        //                            observer.OnNext(true);
        //                            observer.OnCompleted();
        //                        }
        //                        else
        //                        {
        //                            observer.OnNext(false);
        //                            observer.OnCompleted();
        //                        }
        //                    }
        //                    catch (Exception)
        //                    {
        //                        observer.OnNext(false);
        //                        observer.OnCompleted();
        //                    }
        //                },
        //                exception =>
        //                {
        //                    observer.OnNext(false);
        //                    observer.OnCompleted();
        //                });
        //        }
        //        catch(Exception)
        //        {
        //            observer.OnNext(false);
        //            observer.OnCompleted();
        //        }
        //        return disp;
        //    });
        //}

        #endregion




        public WeaveRssServerProxy(
            List<FeedRequest> outgoingFeedRequests)
            //IObserver<IEnumerable<FeedResult>> observer)
        {
            this.outgoingFeedRequests = outgoingFeedRequests;
            //this.observer = observer;

            //GetLatestNews();
        }

        public async Task<List<FeedResult>> GetFeedResultsAsync()
        {
            //try
            //{
                request = HttpWebRequest.CreateHttp(SERVICE_URL);
                request.Method = "POST";
                request.ContentType = "application/json";

                var writeSerializer = new DataContractJsonSerializer(typeof(List<FeedRequest>));

                using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    writeSerializer.WriteObject(requestStream, outgoingFeedRequests);
                    requestStream.Close();
                }

                var response = await request.GetResponseAsync().ConfigureAwait(false);

                var results = ParseFeedResultsFromWebResponse(response);
                return results;
                //observer.OnNext(results);
                //observer.OnCompleted();
            //}
            //catch (Exception exception)
            //{
            //    observer.OnError(exception);
            //}
        }




        #region Helper Methods

        List<FeedResult> ParseFeedResultsFromWebResponse(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            using (var gzip = new GZipInputStream(stream))
            {
                var feedResults = Serializer.Deserialize<List<FeedResult>>(gzip);
                gzip.Close();
                stream.Close();
                return feedResults;
            }
        }

        #endregion




        public void Dispose()
        {
            try
            {
                if (request != null)
                    request.Abort();
            }
            catch (Exception) {}
        }
    }
}