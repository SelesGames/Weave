using System.Collections.Generic;
using System.Linq;
using Weave.RSS.DTOs.Incoming;

namespace weave.Services.RSS
{
    internal static class FeedResultExtensions
    {
        public static FeedUpdateResponse AsFeedUpdateResponse(this FeedResult result)
        {
            var feedUpdate = new FeedUpdateResponse
            {
                Etag = result.Etag,
                LastModified = result.LastModified,
                MostRecentNewsItemPubDate = result.MostRecentNewsItemPubDate,
                OldestNewsItemPubDate = result.OldestNewsItemPubDate,
                News = result.News != null ? result.News
                    .Select(o => new { newsItem = o, pubDateTry = RssDateParsingHelper.TryGetLocalDate(o.PublishDateTime) })
                    .Where(o => o.pubDateTry.Item1)
                    .Select(o =>
                        new NewsItem
                        {
                            Title = o.newsItem.Title,
                            PublishDateTime = o.pubDateTry.Item2,
                            Description = o.newsItem.Description,
                            ImageUrl = o.newsItem.ImageUrl,
                            Link = o.newsItem.Link,

                            // added 10/10/11
                            YoutubeId = o.newsItem.YoutubeId,
                            VideoUri = o.newsItem.VideoUri,
                            PodcastUri = o.newsItem.PodcastUri,
                            ZuneAppId = o.newsItem.ZuneAppId,
                        })
                    .ToList()
                    : new List<NewsItem>(0),
            };
            return feedUpdate;
        }
    }
}
