using SelesGames.Rest;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weave.UserFeedAggregator.Contracts;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;

namespace Weave.UserFeedAggregator.Client
{
    public class Client : IServiceClient
    {
        /// PRODUCTION
        /// ****************************
        const string SERVICE_URL = "http://weave2.cloudapp.net/api/Weave";




        #region User management

        public async Task<Outgoing.UserInfo> AddUserAndReturnNewNews(Incoming.UserInfo incomingUser)
        {
            string append = "create";
            var url = string.Format("{0}", append);

            var client = CreateClient();
            var result = await client.PostAsync<Incoming.UserInfo, Outgoing.UserInfo>(url, incomingUser, CancellationToken.None);
            return result;
        }

        //[HttpGet]
        //public async Task<Outgoing.UserInfo> GetUserInfoWithNoNews(Guid userId)
        //{
        //    var user = await userRepo.Get(userId);
        //    foreach (var feed in user.Feeds)
        //        feed.News = null;

        //    var userBO = ConvertToBusinessObject(user);
        //    var outgoing = ConvertToOutgoing(userBO);
        //    return outgoing;
        //}

        //[HttpGet]
        //public async Task<Outgoing.UserInfo> GetUserInfoWithRefreshedNewsCount(Guid userId)
        //{
        //    var user = await userRepo.Get(userId);
        //    var userBO = ConvertToBusinessObject(user);
        //    await userBO.RefreshAllFeeds();
        //    user = ConvertToDataStore(userBO);
        //    await userRepo.Save(user);
            
        //    foreach (var feed in user.Feeds)
        //        feed.News = null;

        //    userBO = ConvertToBusinessObject(user);
        //    var outgoing = ConvertToOutgoing(userBO);
        //    return outgoing;
        //}

        public async Task<Outgoing.UserInfo> RefreshAndReturnNews(Guid userId)
        {
            string append = "refresh_all";
            var url = string.Format("{0}?userId={1}", append, userId);

            var client = CreateClient();
            var result = await client.GetAsync<Outgoing.UserInfo>(url, CancellationToken.None);
            return result;
        }

        /// <summary>
        /// Refresh only some of the feeds for a given user, then return the full UserInfo graph.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="feedIds">The ids of the feeds to refresh</param>
        /// <returns>UserInfo graph</returns>
        public async Task<Outgoing.UserInfo> RefreshAndReturnNews(Guid userId, List<Guid> feedIds)
        {
            string append = "refresh";
            var url = string.Format("{0}?userId={1}", append, userId);

            var client = CreateClient();
            var result = await client.PostAsync<List<Guid>, Outgoing.UserInfo>(url, feedIds, CancellationToken.None);
            return result;
        }

        #endregion




        #region Feed management

        public async Task AddFeed(Guid userId, Incoming.Feed feed)
        {
            string append = "add_feed";
            var url = string.Format("{0}?userId={1}", append, userId);

            var client = CreateClient();
            await client.PostAsync(url, feed, CancellationToken.None);
        }

        public async Task RemoveFeed(Guid userId, Guid feedId)
        {
            string append = "remove_feed";
            var url = string.Format("{0}?userId={1}&feedId={2}", append, userId, feedId);

            var client = CreateClient();
            await client.GetAsync<object>(url, CancellationToken.None);
        }

        public async Task UpdateFeed(Guid userId, Incoming.Feed feed)
        {
            string append = "update_feed";
            var url = string.Format("{0}?userId={1}", append, userId);

            var client = CreateClient();
            await client.PostAsync(url, feed, CancellationToken.None);
        }

        #endregion




        #region Article management

        public async Task MarkArticleRead(Guid userId, Guid feedId, Guid newsItemId)
        {
            string append = "mark_read";
            var url = string.Format("{0}?userId={1}&feedId={2}&newsItemId={3}", append, userId, feedId, newsItemId);

            var client = CreateClient();
            await client.GetAsync<object>(url, CancellationToken.None);
        }

        public async Task MarkArticleUnread(Guid userId, Guid feedId, Guid newsItemId)
        {
            string append = "mark_unread";
            var url = string.Format("{0}?userId={1}&feedId={2}&newsItemId={3}", append, userId, feedId, newsItemId);

            var client = CreateClient();
            await client.GetAsync<object>(url, CancellationToken.None);

        }

        #endregion




        RestClient CreateClient()
        {
            return new SelesGames.Rest.Protobuf.ProtobufRestClient { UseGzip = true };
        }
    }
}
