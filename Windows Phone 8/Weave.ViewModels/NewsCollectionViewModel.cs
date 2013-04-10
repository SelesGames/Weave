using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Weave.UserFeedAggregator.Contracts;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;
using System.Linq;

namespace Weave.ViewModels
{
    public class NewsCollectionViewModel
    {
        List<Guid> feedIds;
        IServiceClient serviceClient;
        Outgoing.UserInfo userInfo;

        public ObservableCollection<NewsItem> News { get; private set; }
        public int NewArticleCount { get; set; }
        public int UnreadCount { get; set; }
        public int TotalArticleCount { get; set; }

        public NewsCollectionViewModel()
        {
            News = new ObservableCollection<NewsItem>();
        }

        public async Task RefreshNews()
        {
            Outgoing.UserInfo user = null;

            if (feedIds == null)
                user = await serviceClient.RefreshAndReturnNews(userInfo.Id);

            else
                user = await serviceClient.RefreshAndReturnNews(userInfo.Id, feedIds);

            var allNews = user.Feeds
                .SelectMany(o => o.News
                    .Select(n => Convert(n, o)))
                .OrderBy(o => o.LocalDateTime)
                .ToList();

            News.OrderedDescendingUniqueInsert(allNews, o => o.LocalDateTime);
        }

        //void Insert(IList<NewsItem> allNews)
        //{
        //    var newEntryIndex = 0;

        //    for (int i = 0; i < News.Count; i++)
        //    {
        //        var current = allNews[newEntryIndex];
        //        var newsItem = News[i];

        //        if (current.LocalDateTime < newsItem.LocalDateTime)
        //        {
        //            News.Insert(i, current);
        //            newEntryIndex++;
        //        }
        //    }
        //}




        #region Conversion helpers

        NewsItem Convert(Outgoing.NewsItem n, Outgoing.Feed f)
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

        #endregion
    }
}
