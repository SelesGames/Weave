using Microsoft.Phone.Shell;
using System;
using System.Threading.Tasks;
using Weave.ViewModels;
using System.Linq;
using Portable.Common;

namespace Weave.WP.ViewModels.GroupedNews
{
    public class FeedGroup : NewsItemGroup
    {
        UserInfo user;
        CategoryGroup category;
        AllNewsGroup allNews;

        public Feed Feed { get; private set; }

        public FeedGroup(UserInfo user, Feed feed, CategoryGroup category, AllNewsGroup allNews)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (feed == null) throw new ArgumentNullException("feed");
            if (allNews == null) throw new ArgumentNullException("allNews");

            this.user = user;
            this.Feed = feed;
            this.category = category;
            this.allNews = allNews;

            DisplayName = feed.Name;
            NewArticleCount = feed.NewArticleCount;
            UnreadArticleCount = feed.UnreadArticleCount;
            TotalArticleCount = feed.TotalArticleCount;
            FeedCount = 1;
        }

        public override Task<NewsList> GetNewsList(EntryType entryType, Guid? cursorId, int take)
        {
            return user.GetNewsForFeed(Feed.Id, entryType, cursorId, take);
        }

        public override void MarkEntry()
        {
            int prevNewArticleCount = NewArticleCount;
            NewArticleCount = 0;
            Feed.NewArticleCount = 0;
            if (category != null)
            {
                category.NewArticleCount -= prevNewArticleCount;
            }
            allNews.NewArticleCount -= prevNewArticleCount;
        }
        
        public override string GetTeaserPicImageUrl()
        {
            return Feed.TeaserImageUrl;
        }

        public override ShellTile GetShellTile()
        {
            var shellTiles = ShellTile.ActiveTiles;
            return shellTiles.FirstOrDefault(DoesTileMatch);
        }

        bool DoesTileMatch(ShellTile tile)
        {
            if (tile == null || tile.NavigationUri == null || tile.NavigationUri.OriginalString == null)
                return false;

            var uri = tile.NavigationUri;
            var queryParams = uri.ParseQueryString();

            return queryParams.Any(o =>
                o.Key.Equals("feedId", StringComparison.OrdinalIgnoreCase) 
                &&
                Guid.Parse(o.Value) == this.Feed.Id);
        }




        #region Equality/Hash overrides

        public override bool Equals(object obj)
        {
            if (obj is FeedGroup)
            {
                var o = (FeedGroup)obj;
                return Feed.Id.Equals(o.Feed.Id);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Feed.Id.GetHashCode();
        }

        #endregion
    }
}
