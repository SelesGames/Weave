using ICSharpCode.SharpZipLib.GZip;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Weave.RSS.DTOs.Incoming;
using Weave.RSS.DTOs.Outgoing;

namespace Weave.RSS
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
        static readonly TimeSpan pingTimeout = TimeSpan.FromSeconds(4);

        public WeaveRssServerProxy(
            List<FeedRequest> outgoingFeedRequests)
        {
            this.outgoingFeedRequests = outgoingFeedRequests;
        }

        public async Task<List<FeedResult>> GetFeedResultsAsync()
        {
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