using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace Weave.WP.ViewModels.GroupedNews
{
    public class AllNewsGroup : NewsItemGroup
    {
        UserInfo user;
        IEnumerable<NewsItemGroup> subgroups;

        public IEnumerable<NewsItemGroup> Subgroups
        {
            get { return subgroups; }
            set
            {
                subgroups = value;
                if (subgroups == null)
                    return;

                NewArticleCount = subgroups.Sum(o => o.NewArticleCount);
                UnreadArticleCount = subgroups.Sum(o => o.UnreadArticleCount);
                TotalArticleCount = subgroups.Sum(o => o.TotalArticleCount);
                FeedCount = subgroups.Count();
            }
        }

        public AllNewsGroup(UserInfo user)
        {
            if (user == null) throw new ArgumentNullException("user");
            this.user = user;

            DisplayName = "All News";
        }

        public override Task<NewsList> GetNewsList(EntryType entryType, Guid? cursorId, int take)
        {
            return user.GetNewsForCategory("all news", entryType, cursorId, take);
        }

        public override void MarkEntry()
        {
            if (Subgroups == null)
                return;

            foreach (var group in Subgroups)
                group.MarkEntry();

            NewArticleCount = 0;
        }

        static Random r = new Random();

        public override string GetTeaserPicImageUrl()
        {
            if (Subgroups == null)
                return null;

            // RANDOMLY SELECT ONE FEED TO DISPLAY
            var teaserPics = Subgroups.Select(o => o.GetTeaserPicImageUrl()).OfType<string>().ToList();
            if (teaserPics.Any())
            {
                int index = r.Next(0, teaserPics.Count);
                return teaserPics[index];
            }

            return null;
        }
        
        public override ShellTile GetShellTile()
        {
            var shellTiles = ShellTile.ActiveTiles;
            return shellTiles.FirstOrDefault(DoesTileMatch);
        }

        bool DoesTileMatch(ShellTile tile)
        {
            if (tile == null || tile.NavigationUri == null)
                return false;

            var uri = tile.NavigationUri.OriginalString;

            return string.IsNullOrEmpty(uri) || !uri.Contains("?");
        }
    }
}
