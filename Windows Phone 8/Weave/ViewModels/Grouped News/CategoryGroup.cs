using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;
using Portable.Common;

namespace weave
{
    public class CategoryGroup : NewsItemGroup
    {
        UserInfo user;
        string category;

        public IReadOnlyList<FeedGroup> Feeds { get; private set; }




        #region Constructors

        public CategoryGroup(UserInfo user, string category, IEnumerable<FeedGroup> feeds)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (category == null) throw new ArgumentNullException("category");
            if (feeds == null) throw new ArgumentNullException("feeds");

            Initialize(user, category, feeds);
        }

        public CategoryGroup(UserInfo user, string category, IEnumerable<Feed> feeds, AllNewsGroup allNews)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (category == null) throw new ArgumentNullException("category");
            if (feeds == null) throw new ArgumentNullException("feeds");

            Initialize(user, category, feeds.Select(o => new FeedGroup(user, o, this, allNews)));
        }

        void Initialize(UserInfo user, string category, IEnumerable<FeedGroup> feeds)
        {
            this.user = user;
            this.category = category;
            this.Feeds = new List<FeedGroup>(feeds);

            DisplayName = category;

            NewArticleCount = Feeds.Sum(o => o.NewArticleCount);
            UnreadArticleCount = Feeds.Sum(o => o.UnreadArticleCount);
            TotalArticleCount = Feeds.Sum(o => o.TotalArticleCount);
            FeedCount = Feeds.Count;
        }

        #endregion




        public override Task<NewsList> GetNewsList(EntryType entryType, int skip, int take)
        {
            return user.GetNewsForCategory(category, entryType, skip, take);
        }

        static Random r = new Random();

        public override string GetTeaserPicImageUrl()
        {     
            // RANDOMLY SELECT ONE FEED TO DISPLAY
            var teaserPics = Feeds.Select(o => o.GetTeaserPicImageUrl()).OfType<string>().ToList();
            if (teaserPics.Any())
            {
                int index = r.Next(0, teaserPics.Count);
                return teaserPics[index];
            }

            return null;
        }

        protected override void OnMarkEntry()
        {
            foreach (var feed in Feeds)
                feed.MarkEntry();

            NewArticleCount = 0;
        }

        protected override ShellTile GetShellTile()
        {
            var shellTiles = ShellTile.ActiveTiles;
            return shellTiles.FirstOrDefault(DoesTileMatch);
        }

        bool DoesTileMatch(ShellTile tile)
        {
            if (tile == null || tile.NavigationUri == null || tile.NavigationUri.OriginalString == null)
                return false;

            var uri = tile.NavigationUri;
            var queryParams = uri.ParseQueryString().Memoize();

            if (queryParams.Any(o => o.Key.Equals("mode", StringComparison.OrdinalIgnoreCase)))
            {
                return queryParams.Any(o =>
                    o.Key.Equals("header", StringComparison.OrdinalIgnoreCase)
                    &&
                    o.Value.Equals(this.category, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }




        #region Equality/Hash overrides

        public override bool Equals(object obj)
        {
            if (category == null)
                return false;

            if (obj is CategoryGroup)
            {
                var o = (CategoryGroup)obj;
                return category.Equals(o.category, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return category == null ? -1 : category.GetHashCode();
        }

        #endregion
    }
}
