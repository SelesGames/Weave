using System;
using System.Collections.Generic;
using System.Linq;

namespace weave
{
    public static class ExtensionMethods
    {
        public static IEnumerable<string> UniqueCategoryNames(this IEnumerable<FeedSource> feeds)
        {
            return feeds.Select(o => o.Category).Distinct().OfType<string>();
        }

        public static IEnumerable<FeedSource> OfCategory(this IEnumerable<FeedSource> feeds, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return feeds;

            return feeds.Where(o => categoryName.Equals(o.Category, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<NewsItem> AllNews(this IEnumerable<FeedSource> feeds)
        {
            return feeds.Where(o => o.News != null).SelectMany(o => o.News);
        }

        public static IEnumerable<NewsItem> AllOrderedNews(this IEnumerable<FeedSource> feeds)
        {
            return feeds.AllNews().OrderByDescending(o => o.PublishDateTime);
        }

        public static bool AreThereTooManyFeeds(this IEnumerable<FeedSource> feeds)
        {
            return feeds.Count() > weave.Data.Weave4DataAccessLayer.MaxAllowedSources;
        }

        //public static IEnumerable<NewsItem> AllReverseOrderedNews(this IEnumerable<FeedSource> feeds)
        //{
        //    return feeds.AllNews().OrderBy(o => o.PublishDateTime);
        //}

        //public static IObservable<IEnumerable<FeedSource>> GetRefreshedFeeds(this IEnumerable<FeedSource> feeds)
        //{
        //    return feeds.GetRefreshedFeeds(TimeSpan.FromSeconds(12));
        //}

        //public static IObservable<IEnumerable<FeedSource>> GetRefreshedFeeds(this IEnumerable<FeedSource> feeds, TimeSpan bufferTime)
        //{
        //    if (feeds == null)
        //        throw new ArgumentNullException("feeds in FeedSourceExtensions.GetRefreshedFeeds");

        //    return feeds
        //        .Select(feed => feed.NewsWasRefreshedNotificationStream.Select(_ => feed))
        //        .Merge()
        //        .Buffer(bufferTime)
        //        .Where(buffer => buffer.Count > 0)
        //        .Select(o => (IEnumerable<FeedSource>)o);
        //}

        //public static IObservable<EventPattern<EventArgs>> GetIsNewsUpdatingChanged(this FeedSource feed)
        //{
        //    return Observable.FromEventPattern<EventArgs>(feed, "IsUpdatingChanged");
        //}
    }
}
