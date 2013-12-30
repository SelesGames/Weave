using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace Weave.WP.ViewModels.GroupedNews
{
    public abstract class NewsItemGroup : ViewModelBase
    {
        #region Private member variables

        string displayName, imageSource;
        int feedCount, newArticleCount, unreadArticleCount, totalArticleCount;

        #endregion




        #region Properties

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; Raise("DisplayName"); }
        }

        public int FeedCount
        {
            get { return feedCount; }
            set { feedCount = value; Raise("FeedCount"); }
        }

        public int NewArticleCount
        {
            get { return newArticleCount; }
            set { newArticleCount = value; Raise("NewArticleCount", "NewArticleCountText", "NewArticleCountBrush"); }
        }

        public int UnreadArticleCount
        {
            get { return unreadArticleCount; }
            set { unreadArticleCount = value; Raise("UnreadArticleCount"); }
        }

        public int TotalArticleCount
        {
            get { return totalArticleCount; }
            set { totalArticleCount = value; Raise("TotalArticleCount"); }
        }

        public string ImageSource
        {
            get { return imageSource; }
            set { imageSource = value; Raise("ImageSource"); }
        }

        #endregion




        #region Derived Properties

        public string NewArticleCountText
        {
            get
            {
                return NewArticleCount > 0 ? NewArticleCount.ToString() : null;
            }
        }

        public string DisplayNameUppercase
        {
            get { return DisplayName == null ? null : DisplayName.ToUpper(); }
        }

        public string DisplayNameLowercase
        {
            get { return DisplayName == null ? null : DisplayName.ToLower(); }
        }

        #endregion




        public async Task<IEnumerable<NewsItem>> GetNews(EntryType entryType, int skip, int take)
        {
            var news = await GetNewsList(entryType, skip, take);

            FeedCount = news.FeedCount;
            NewArticleCount = news.NewArticleCount;
            UnreadArticleCount = news.UnreadArticleCount;
            TotalArticleCount = news.TotalArticleCount;

            return news.News;
        }

        public abstract Task<NewsList> GetNewsList(EntryType entryType, int skip, int take);
        public abstract void MarkEntry();
        public abstract string GetTeaserPicImageUrl();
        public abstract ShellTile GetShellTile();

        public override string ToString()
        {
            return DisplayName.ToString();
        }
    }
}