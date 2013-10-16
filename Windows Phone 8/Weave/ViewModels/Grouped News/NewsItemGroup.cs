using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Weave.ViewModels;

namespace weave
{
    public abstract class NewsItemGroup : ViewModelBase
    {
        #region static media values that come from App.Resources

        static bool areArticleBrushesSet = false, areFontFamiliesSet = false;
        static Brush newArticleBrush, noNewArticleBrush;
        static FontFamily categoryFont, feedFont;

        static void SetArticleBrushes()
        {
            newArticleBrush = Application.Current.Resources["PhoneAccentBrush"] as Brush;
            noNewArticleBrush = Application.Current.Resources["PhoneSubtleBrush"] as Brush;
            areArticleBrushesSet = true;
        }

        static void SetFontFamilies()
        {
            categoryFont = Application.Current.Resources["PhoneFontFamilyBlack"] as FontFamily;
            feedFont = Application.Current.Resources["PhoneFontFamilyNormal"] as FontFamily;
            areFontFamiliesSet = true;
        }

        #endregion


        string displayName;
        int feedCount, newArticleCount, unreadArticleCount, totalArticleCount;



        //public string Name { get; set; }
        //public Guid FeedId { get; set; }


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

        public string ImageSource { get; set; }

        #endregion




        #region Derived Properties

        public string NewArticleCountText
        {
            get
            {
                return NewArticleCount > 0 ? NewArticleCount.ToString() : null;
            }
        }

        public Brush NewArticleCountBrush
        {
            get
            {
                if (!areArticleBrushesSet)
                    SetArticleBrushes();

                return NewArticleCount > 0 ? newArticleBrush : noNewArticleBrush;
            }
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

        //public CategoryOrFeedType Type { get; set; }


        //public FontFamily NameFontFamily
        //{
        //    get
        //    {
        //        if (!areFontFamiliesSet)
        //            SetFontFamilies();

        //        return Type == CategoryOrFeedType.Category ? categoryFont : feedFont;
        //    }
        //}

        //public override bool Equals(object obj)
        //{
        //    if (Name == null)
        //        return false;

        //    if (obj is NewsItemGroup)
        //    {
        //        var vm = (NewsItemGroup)obj;

        //        // if they are both Categories, compare them by name
        //        if (Type == CategoryOrFeedType.Category && vm.Type == CategoryOrFeedType.Category)
        //        {
        //            return Name.Equals(vm.Name) && Type.Equals(vm.Type);
        //        }
        //        // if they are both Feeds, compare them by FeedId
        //        else if (Type == CategoryOrFeedType.Feed && vm.Type == CategoryOrFeedType.Feed)
        //        {
        //            return FeedId.Equals(vm.FeedId);
        //        }
        //    }

        //    return false;
        //}

        //public override int GetHashCode()
        //{
        //    return Name == null ? -1 : Name.GetHashCode();
        //}

        public override string ToString()
        {
            return DisplayName.ToString();
        }

        //public enum CategoryOrFeedType
        //{
        //    Category,
        //    Feed
        //}
    }
}
