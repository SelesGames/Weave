using System.Threading.Tasks;
using Weave.ViewModels;

namespace Weave.WP.ViewModels.GroupedNews
{
    public class FavoriteArticlesGroup : ArticleGroup
    {
        UserInfo user;

        public FavoriteArticlesGroup(UserInfo user)
        {
            this.user = user;

            DisplayName = "Favorites";
            NewArticleCount = -1;
            UnreadArticleCount = -1;
            TotalArticleCount = 9999;
            FeedCount = -1;
        }

        public async override Task<NewsList> GetNewsList(EntryType entryType, int skip, int take)
        {
            var favorites = await user.GetFavorites(skip, take);

            var newsList = new NewsList
            {
                FeedCount = -1,
                NewArticleCount = -1,
                UnreadArticleCount = -1,
                TotalArticleCount = 9999,
                IncludedArticleCount = favorites.Count,
                Skip = skip,
                Take = take,
                News = favorites,
            };

            return newsList;
        }

        public override void MarkEntry() { }

        public override string GetTeaserPicImageUrl()
        {
            return null;
        }

        public override global::Microsoft.Phone.Shell.ShellTile GetShellTile()
        {
            return null;
        }
    }
}
