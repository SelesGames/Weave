using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave
{
    public class CategoryGroup : NewsItemGroup
    {
        UserInfo user;
        string category;
        IEnumerable<FeedGroup> feeds;




        #region Constructors

        public CategoryGroup(UserInfo user, string category, IEnumerable<FeedGroup> feeds)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (category == null) throw new ArgumentNullException("category");
            if (feeds == null) throw new ArgumentNullException("feeds");

            this.user = user;
            this.category = category;
            this.feeds = feeds ?? new List<FeedGroup>();

            DisplayName = category;
        }

        public CategoryGroup(UserInfo user, string category, IEnumerable<Feed> feeds)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (category == null) throw new ArgumentNullException("category");
            if (feeds == null) throw new ArgumentNullException("feeds");

            this.user = user;
            this.category = category;
            this.feeds = feeds.Select(o => new FeedGroup(user, o, this));
        }

        #endregion




        public override Task<NewsList> GetNewsList(EntryType entryType, int skip, int take)
        {
            return user.GetNewsForCategory(category, entryType, skip, take);
        }

        public override void MarkEntry()
        {
            NewArticleCount = 0;
            foreach (var feed in feeds)
                feed.MarkEntry();
        }

        static Random r = new Random();

        public override string GetTeaserPicImageUrl()
        {
            IEnumerable<FeedGroup> eligibleFeeds;

            if (category != null && category.Equals("all news", StringComparison.OrdinalIgnoreCase))
            {
                eligibleFeeds = feeds;
            }
            else
            {
                eligibleFeeds = feeds.Where(o => category.Equals(o.Feed.Category, StringComparison.OrdinalIgnoreCase));
            }
            
            // TODO: RANDOMLY SELECT ONE FEED TO DISPLAY
            var teaserPics = eligibleFeeds.Select(o => o.GetTeaserPicImageUrl()).OfType<string>().ToList();
            if (teaserPics.Any())
            {
                int index = r.Next(0, teaserPics.Count);
                return teaserPics[index];
            }

            return null;
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
