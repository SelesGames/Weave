using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weave.RSS.DTOs.Incoming;
using Weave.RSS.DTOs.Outgoing;

namespace Weave.RSS
{
    internal class WeaveRssServerProxy
    {
        /// LOCAL
        /// ****************************
        //const string SERVICE_URL = "http://192.168.1.72/api/Weave";
        //const string PING_URL = "http://192.168.1.72/api/Ping/";


        /// STAGING
        /// ****************************
        //const string SERVICE_URL = "http://weave2.cloudapp.net/api/Weave";
        //const string PING_URL = "http://5c6ea5aab3a74d64ad1ba9bdb3718f13.cloudapp.net:8086/Ping/";


        /// PRODUCTION
        /// ****************************
        const string SERVICE_URL = "http://weave2.cloudapp.net/api/Weave";
        const string PING_URL = "http://weave2.cloudapp.net/Ping/";


        public async Task<List<FeedResult>> GetFeedResultsAsync(List<FeedRequest> outgoingFeedRequests)
        {
            var client = new SelesGames.Rest.Protobuf.ProtobufRestClient { UseGzip = true };
            var results = await client.PostAsync<List<FeedRequest>, List<FeedResult>>(SERVICE_URL, outgoingFeedRequests, CancellationToken.None);
            return results;
        }
    }
}