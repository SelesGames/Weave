using SelesGames;
using System;
using System.Threading.Tasks;
using Weave.ViewModels.Contracts.Client;

namespace Weave.ViewModels
{
    public abstract class BaseNewsCollectionViewModel
    {
        protected IUserCache userCache = ServiceResolver.Get<IUserCache>();

        public abstract Task<NewsList> GetNewsList(bool refresh = false, bool markEntry = false, int skip = 0, int take = 10);
    }

    public class NewsCollectionCategoryViewModel : BaseNewsCollectionViewModel
    {
        string category;

        public NewsCollectionCategoryViewModel(string category)
        {
            this.category = category;
        }

        public override Task<NewsList> GetNewsList(bool refresh = false, bool markEntry = false, int skip = 0, int take = 10)
        {
            var user = userCache.Get();
            return user.GetNewsForCategory(category, refresh, markEntry, skip, take);
        }
    }

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
