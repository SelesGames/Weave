using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.UserFeedAggregator.Contracts;
using Weave.ViewModels.Contracts.Client;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;


namespace Weave.ViewModels.Cache
{
    public class ServiceClient : IViewModelRepository
    {
        IWeaveUserService innerClient;
        Guid userId;

        public ServiceClient(Guid userId, IWeaveUserService innerClient)
        {
            this.userId = userId;
            this.innerClient = innerClient;
        }

        public async Task<IList<NewsItem>> GetNews(string category, bool refresh = false, int skip = 0, int take = 10)
        {
            var userNews = await innerClient.GetNews(userId, category, refresh, skip, take);
            return CreateDistinctAndOrdered(userNews);
        }

        public async Task<IList<NewsItem>> GetNews(Guid feedId, bool refresh = false, int skip = 0, int take = 10)
        {
            var userNews = await innerClient.GetNews(userId, feedId, refresh, skip, take);
            return CreateDistinctAndOrdered(userNews);
        }

        public async Task<IList<NewsItem>> GetFeaturedNews(string category, int take, bool refresh = false)
        {
            var featuredNews = await innerClient.GetFeaturedNews(userId, category, take, refresh);
            return CreateOrdered(featuredNews);
        }

        public async Task<IList<NewsItem>> GetFeaturedNews(Guid feedId, int take, bool refresh = false)
        {
            var featuredNews = await innerClient.GetFeaturedNews(userId, feedId, take, refresh);
            return CreateOrdered(featuredNews);
        }

        public Task AddFeed(Feed feed)
        {
            return innerClient.AddFeed(userId, ConvertToNewFeed(feed));
        }

        public Task RemoveFeed(Feed feed)
        {
            return innerClient.RemoveFeed(userId, feed.Id);
        }

        public Task UpdateFeed(Feed feed)
        {
            return innerClient.UpdateFeed(userId, ConvertToUpdatedFeed(feed));
        }

        public Task MarkArticleRead(NewsItem newsItem)
        {
            return innerClient.MarkArticleRead(userId, newsItem.Feed.Id, newsItem.Id);
        }

        public Task MarkArticleUnread(NewsItem newsItem)
        {
            return innerClient.MarkArticleUnread(userId, newsItem.Feed.Id, newsItem.Id);
        }




        #region Helper methods

        IList<NewsItem> CreateDistinctAndOrdered(Outgoing.UserNews userNews)
        {
            var allNews = userNews.Feeds
                .SelectMany(o => o.News
                    .Select(n => Convert(n, o)))
                .OrderBy(o => o.LocalDateTime)
                .Distinct(NewsItemComparer.Instance)
                .ToList();

            return allNews;
        }

        IList<NewsItem> CreateOrdered(Outgoing.LiveTileNewsList liveTileNewsList)
        {
            var allNews = liveTileNewsList.Feeds
                .SelectMany(o => o.News
                    .Select(n => Convert(n, o)))
                .OrderBy(o => o.LocalDateTime)
                .ToList();

            return allNews;
        }

        #endregion




        #region Conversion helpers

        protected NewsItem Convert(Outgoing.NewsItem n, Outgoing.Feed f)
        {
            return new NewsItem
            {
                Id = n.Id,
                Title = n.Title,
                Link = n.Link,
                UtcPublishDateTime = n.UtcPublishDateTime,
                ImageUrl = n.ImageUrl,
                YoutubeId = n.YoutubeId,
                VideoUri = n.VideoUri,
                PodcastUri = n.PodcastUri,
                ZuneAppId = n.ZuneAppId,
                OriginalDownloadDateTime = n.OriginalDownloadDateTime,
                Image = Convert(n.Image),
                Feed = Convert(f),
            };
        }

        Feed Convert(Outgoing.Feed o)
        {
            return new Feed
            {
                Id = o.Id,
                Name = o.Name,
                Uri = o.Uri,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        Image Convert(Outgoing.Image o)
        {
            return new Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        Incoming.NewFeed ConvertToNewFeed(Feed o)
        {
            return new Incoming.NewFeed
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (Weave.UserFeedAggregator.DTOs.ArticleViewingType)o.ArticleViewingType,
            };
        }

        Incoming.UpdatedFeed ConvertToUpdatedFeed(Feed o)
        {
            return new Incoming.UpdatedFeed
            {
                Id = o.Id,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (Weave.UserFeedAggregator.DTOs.ArticleViewingType)o.ArticleViewingType, 
            };
        }

        #endregion
    }
}
