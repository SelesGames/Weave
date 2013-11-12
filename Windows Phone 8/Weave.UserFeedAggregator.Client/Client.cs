﻿using SelesGames.Rest;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weave.User.Service.Contracts;
using Incoming = Weave.User.Service.DTOs.ServerIncoming;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;

namespace Weave.User.Service.Client
{
    public class Client : IWeaveUserService
    {
        /// PRODUCTION
        /// ****************************
        const string SERVICE_URL = "http://weave-user.cloudapp.net/api/user/";




        #region User creation

        public async Task<Outgoing.UserInfo> AddUserAndReturnUserInfo(Incoming.UserInfo incomingUser)
        {
            string append = "create";
            var url = SERVICE_URL + append;

            var client = CreateClient();
            var result = await client.PostAsync<Incoming.UserInfo, Outgoing.UserInfo>(url, incomingUser, CancellationToken.None);
            return result;
        }

        #endregion




        #region Get Basic User Info (suitable for panorama home screen)

        public async Task<Outgoing.UserInfo> GetUserInfo(Guid userId, bool refresh = false)
        {
            string append = "info";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("refresh", refresh)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<Outgoing.UserInfo>(url, CancellationToken.None);
            return result;
        }

        #endregion




        #region Get News for User (either by category or feedId)

        public async Task<Outgoing.NewsList> GetNews(Guid userId,  string category, EntryType entry = EntryType.Peek, int skip = 0, int take = 10, DTOs.NewsItemType type = DTOs.NewsItemType.Any, bool requireImage = false)
        {
            string append = "news";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("category", category)
                .AddParameter("entry", entry)
                .AddParameter("skip", skip)
                .AddParameter("take", take)
                .AddParameter("type", (int)type)
                .AddParameter("requireImage", requireImage)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();

            var result = await client.GetAsync<Outgoing.NewsList>(url, CancellationToken.None);
            return result;
        }

        public async Task<Outgoing.NewsList> GetNews(Guid userId, Guid feedId, EntryType entry = EntryType.Peek, int skip = 0, int take = 10, DTOs.NewsItemType type = DTOs.NewsItemType.Any, bool requireImage = false)
        {
            string append = "news";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("feedId", feedId)
                .AddParameter("entry", entry)
                .AddParameter("skip", skip)
                .AddParameter("take", take)
                .AddParameter("type", (int)type)
                .AddParameter("requireImage", requireImage)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();

            var result = await client.GetAsync<Outgoing.NewsList>(url, CancellationToken.None);
            return result;
        }

        #endregion




        #region Feed management

        public Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, bool refresh = false, bool nested = false)
        {
            string append = "feeds";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("refresh", refresh)
                .AddParameter("nested", nested)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            return client.GetAsync<Outgoing.FeedsInfoList>(url, CancellationToken.None);
        }

        public Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, string category, bool refresh = false, bool nested = false)
        {
            string append = "feeds";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("category", category)
                .AddParameter("refresh", refresh)
                .AddParameter("nested", nested)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            return client.GetAsync<Outgoing.FeedsInfoList>(url, CancellationToken.None);
        }

        public Task<Outgoing.FeedsInfoList> GetFeeds(Guid userId, Guid feedId, bool refresh = false, bool nested = false)
        {
            string append = "feeds";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("feedId", feedId)
                .AddParameter("refresh", refresh)
                .AddParameter("nested", nested)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            return client.GetAsync<Outgoing.FeedsInfoList>(url, CancellationToken.None);
        }

        public Task<Outgoing.Feed> AddFeed(Guid userId, Incoming.NewFeed feed)
        {
            string append = "add_feed";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .ToString();

            var client = CreateClient();
            return client.PostAsync<Incoming.NewFeed, Outgoing.Feed>(url, feed, CancellationToken.None);
        }

        public async Task RemoveFeed(Guid userId, Guid feedId)
        {
            string append = "remove_feed";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("feedId", feedId)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            await client.GetAsync(url, CancellationToken.None);
        }

        public async Task UpdateFeed(Guid userId, Incoming.UpdatedFeed feed)
        {
            string append = "update_feed";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .ToString();

            var client = CreateClient();
            await client.PostAsync(url, feed, CancellationToken.None);
        }

        public async Task BatchChange(Guid userId, Incoming.BatchFeedChange changeSet)
        {
            string append = "batch_change";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .ToString();

            var client = CreateClient();
            await client.PostAsync(url, changeSet, CancellationToken.None);
        }

        #endregion




        #region Article management

        public async Task MarkArticleRead(Guid userId, Guid newsItemId)
        {
            string append = "mark_read";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("newsItemId", newsItemId)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            await client.GetAsync(url, CancellationToken.None);
        }

        public async Task MarkArticleUnread(Guid userId, Guid newsItemId)
        {
            string append = "mark_unread";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("newsItemId", newsItemId)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            await client.GetAsync(url, CancellationToken.None);
        }

        public async Task MarkArticlesSoftRead(Guid userId, List<Guid> newsItemIds)
        {
            string append = "soft_read";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .ToString();

            var client = CreateClient();
            await client.PostAsync(url, newsItemIds, CancellationToken.None);
        }

        public async Task MarkArticlesSoftRead(Guid userId, string category)
        {
            string append = "soft_read";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("category", category)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            await client.GetAsync(url, CancellationToken.None);
        }

        public async Task MarkArticlesSoftRead(Guid userId, Guid feedId)
        {
            string append = "soft_read";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("feedId", feedId)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            await client.GetAsync(url, CancellationToken.None);
        }

        public async Task AddFavorite(Guid userId, Guid newsItemId)
        {
            string append = "add_favorite";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("newsItemId", newsItemId)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            await client.GetAsync(url, CancellationToken.None);
        }

        public async Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            string append = "remove_favorite";
            var url = new UriBuilder(SERVICE_URL + append)
                .AddParameter("userId", userId)
                .AddParameter("newsItemId", newsItemId)
                .AddCacheBuster()
                .ToString();

            var client = CreateClient();
            await client.GetAsync(url, CancellationToken.None);
        }

        #endregion




        RestClient CreateClient()
        {
            //return new SelesGames.Rest.Protobuf.ProtobufRestClient { UseGzip = true };
            return new SelesGames.Rest.JsonDotNet.JsonDotNetRestClient { UseGzip = true };
        }
    }
}
