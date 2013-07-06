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

        public override Task<NewsList> GetNewsList(EntryType entry, int skip, int take)
        {
            var user = userCache.Get();
            return user.GetNewsForFeed(feedId, entry, skip, take);
        }
    }
}
