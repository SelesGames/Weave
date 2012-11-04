using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace weave
{
    public class CategoryRefreshListener : RefreshListenerBase
    {
        string categoryName;

        public CategoryRefreshListener(string categoryName)
        {
            this.categoryName = categoryName;
        }

        public async override Task GetRefreshed()
        {
            var categoryFeeds = GetCategoryFeeds();
            if (categoryFeeds != null)
                await categoryFeeds.Select(o => o.CurrentRefresh).OfType<Task>();
        }

        public override int GetNewCount()
        {
            var categoryFeeds = GetCategoryFeeds();
            if (categoryFeeds == null)
                return 0;

            return categoryFeeds.AllNews().Where(o => o.IsNew()).Count();
                //.Select(feed => feed.News)
                //.OfType<List<NewsItem>>()
                //.Aggregate(0, (seed, i) => seed += i.Count);
        }

        IEnumerable<FeedSource> GetCategoryFeeds()
        {
            if (categoryName == null)
                return null;

            var feeds = DataAccessLayer.Feeds.Get().WaitOnResult();

            IEnumerable<FeedSource> categoryFeeds;
            if (categoryName.Equals("all news", StringComparison.OrdinalIgnoreCase))
                categoryFeeds = feeds;
            else
                categoryFeeds = feeds.OfCategory(categoryName);

            return categoryFeeds;
        }
    }
}
