using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave
{
    public class NewsCollectionCategoryViewModel : BaseNewsCollectionViewModel
    {
        string category;

        public NewsCollectionCategoryViewModel(string category)
        {
            this.category = category;
        }

        public override Task<NewsList> GetNewsList(bool refresh, bool markEntry, int skip, int take)
        {
            var user = userCache.Get();
            return user.GetNewsForCategory(category, refresh, markEntry, skip, take);
        }
    }
}
