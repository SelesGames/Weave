using System;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave
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

        public override Task<NewsList> GetNewsList(EntryType entryType, int skip, int take)
        {
            return user.GetNewsForFeed(Feed.Id, entryType, skip, take);
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
