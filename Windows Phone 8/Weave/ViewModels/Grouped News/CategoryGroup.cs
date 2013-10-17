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

        public IReadOnlyList<FeedGroup> Feeds { get; private set; }




        #region Constructors

        public CategoryGroup(UserInfo user, string category, IEnumerable<FeedGroup> feeds)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (category == null) throw new ArgumentNullException("category");
            if (feeds == null) throw new ArgumentNullException("feeds");

            Initialize(user, category, feeds);
        }

        public CategoryGroup(UserInfo user, string category, IEnumerable<Feed> feeds)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (category == null) throw new ArgumentNullException("category");
            if (feeds == null) throw new ArgumentNullException("feeds");

            Initialize(user, category, feeds.Select(o => new FeedGroup(user, o, this)));
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

        public override void MarkEntry()
        {
            NewArticleCount = 0;
            foreach (var feed in Feeds)
                feed.MarkEntry();
        }

        static Random r = new Random();

        public override string GetTeaserPicImageUrl()
        {
            IEnumerable<FeedGroup> eligibleFeeds;

            if (category != null && category.Equals("all news", StringComparison.OrdinalIgnoreCase))
            {
                eligibleFeeds = Feeds;
            }
            else
            {
                eligibleFeeds = Feeds.Where(o => category.Equals(o.Feed.Category, StringComparison.OrdinalIgnoreCase));
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
