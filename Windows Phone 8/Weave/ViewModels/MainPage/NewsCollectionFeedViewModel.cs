using System;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave
{
    public class NewsCollectionFeedViewModel : BaseNewsCollectionViewModel
    {
        Guid feedId;

        public NewsCollectionFeedViewModel(Guid feedId)
        {
            this.feedId = feedId;
        }

        public override Task<NewsList> GetNewsList(bool refresh = false, bool markEntry = false, int skip = 0, int take = 10)
        {
            var user = userCache.Get();
            return user.GetNewsForFeed(feedId, refresh, markEntry, skip, take);
        }
    }
}
