using System.Collections.Generic;
using System.Linq;


namespace weave
{
    public class NewsItemService
    {
        public static IEnumerable<NewsItem> GetAllNews()
        {
            return GetDesiredFeeds()
                .Where(o => o.News != null)
                .SelectMany(o => o.News)
                .OrderByDescending(o => o.PublishDateTime);
        }

        public static IEnumerable<NewsItem> ByCategory(string categoryName)
        {
            return GetDesiredFeeds()
                .Where(o => o.Category == categoryName)
                .Where(o => o.News != null)
                .SelectMany(o => o.News)
                .OrderByDescending(o => o.PublishDateTime);
        }

        public static IEnumerable<FeedSource> GetEnabledFeeds()
        {
            return GetDesiredFeeds().Where(o => o.News != null);
        }

        public static IEnumerable<FeedSource> GetEnabledFeedsByCategory(string categoryName)
        {
            return GetDesiredFeeds()
                .Where(o => o.Category == categoryName)
                .Where(o => o.News != null);
        }

        public static void RefreshAllFeeds()
        {
            GetDesiredFeeds().ToList().ForEach(feed => feed.CheckForAndGetNewFeedData());
        }



        #region Private helper methods

        static IEnumerable<FeedSource> GetDesiredFeeds()
        {
            return FeedsSettingsService.GetAllFeeds()
                .Where(feed => feed.IsEnabled)
                .Where(feed => CheckIfCategoryIsEnabledOrIfNoCategory(feed));
        }

        static bool CheckIfCategoryIsEnabledOrIfNoCategory(FeedSource feed)
        {
            if (string.IsNullOrEmpty(feed.Category))
                return true;

            var category = FeedsSettingsService.GetCategory(feed.Category);
            if (category == null)
                return true;
            return category.IsEnabled;
        }

        #endregion
    }
}
