using System.Collections.Generic;
using System.Linq;

namespace weave
{
    public static class FeedSourceExtensions
    {
        public static IEnumerable<CategoryOrLooseFeedViewModel> GetAllSources(this IEnumerable<FeedSource> feeds)
        {
            var categories = feeds.UniqueCategoryNames()
                .Select(o => o.ToLower())
                .OrderBy(o => o)
                .Select(o => new CategoryOrLooseFeedViewModel { Name = o, Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category });

            var looseFeeds = feeds
                .Where(o => string.IsNullOrEmpty(o.Category) && o.FeedName != null)
                .Select(o => new { o, name = o.FeedName.ToLower() })
                .OrderBy(o => o.name)
                .Select(o => new CategoryOrLooseFeedViewModel { Name = o.name, Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed, FeedId = o.o.Id });


            var sources = new List<CategoryOrLooseFeedViewModel>();
            sources.Add(new CategoryOrLooseFeedViewModel { Name = "all news", Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category });
            sources.AddRange(categories.Union(looseFeeds));

            return sources;
        }
    }
}
