using SelesGames;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;

namespace weave
{
    public abstract class BaseNewsCollectionViewModel
    {
        protected IUserCache userCache = ServiceResolver.Get<IUserCache>();

        public abstract Task<NewsList> GetNewsList(bool refresh, bool markEntry, int skip, int take);
    }
}
