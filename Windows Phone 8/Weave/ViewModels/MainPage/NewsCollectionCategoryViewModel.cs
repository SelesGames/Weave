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

        public override Task<NewsList> GetNewsList(bool refresh = false, bool markEntry = false, int skip = 0, int take = 10)
        {
            var user = userCache.Get();
            return user.GetNewsForCategory(category, refresh, markEntry, skip, take);
        }
    }
}
