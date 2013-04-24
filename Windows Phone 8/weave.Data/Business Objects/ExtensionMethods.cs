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

        //public static IEnumerable<NewsItem> AllNews(this IEnumerable<FeedSource> feeds)
        //{
        //    return feeds.Where(o => o.News != null).SelectMany(o => o.News);
        //}

        //public static IEnumerable<NewsItem> AllOrderedNews(this IEnumerable<FeedSource> feeds)
        //{
        //    return feeds.AllNews().OrderByDescending(o => o.PublishDateTime);
        //}

        public static bool AreThereTooManyFeeds(this IEnumerable<FeedSource> feeds)
        {
            return feeds.Count() > weave.Data.Weave4DataAccessLayer.MaxAllowedSources;
        }
    }
}
