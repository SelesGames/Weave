using System;
using System.ComponentModel;
using Weave.Customizability;
using Weave.User;

namespace weave
{
    public class PermanentState : INotifyPropertyChanged
    {
        public bool IsFirstTime { get; set; }
        //public UserInfo User { get; set; }
        public Guid? UserId { get; set; }
        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }
        public string ArticleDeletionTimeForMarkedRead { get; set; }
        public string ArticleDeletionTimeForUnread { get; set; }
        public bool IsHideAppBarOnArticleListPageEnabled { get; set; }
        public bool IsHideAppBarOnArticleViewerPageEnabled { get; set; }
        public bool IsSystemTrayVisibleWhenPossible { get; set; }
        public weave.Services.MostViewedHistory.RunHistory RunHistory { get; set; }
        public string SpeakTextVoice { get; set; }




        #region Settings that tweak the MainPage (aka Article List) view

        public ArticleListFormatType ArticleListFormat
        {
            get { return this.articleListFormat; }
            set
            {
                if (this.articleListFormat != value)
                {
                    this.articleListFormat = value;
                    PropertyChanged.Raise(this, "ArticleListFormat");
                }
            }
        }
        ArticleListFormatType articleListFormat;

        public FontSize ArticleListFontSize
        {
            get { return this.articleListFontSize; }
            set
            {
                if (this.articleListFontSize != value)
                {
                    this.articleListFontSize = value;
                    PropertyChanged.Raise(this, "ArticleListFontSize");
                }
            }
        }
        FontSize articleListFontSize;

        public string ArticleListFontName
        {
            get { return this.articleListFontName; }
            set
            {
                if (this.articleListFontName != value)
                {
                    this.articleListFontName = value;
                    PropertyChanged.Raise(this, "ArticleListFontName");
                }
            }
        }
        string articleListFontName;

        #endregion




        #region Settings that tweak the Article View

        public string CurrentTheme { get; set; }
        public FontSize ArticleViewFontSize { get; set; }
        public string ArticleViewFontName { get; set; }

        #endregion




        public PermanentState()
        {
            IsFirstTime = true; // start off as true

            // DEFAULT ARTICLE LIST SETTINGS
            IsHideAppBarOnArticleListPageEnabled = true;
            ArticleListFormat = ArticleListFormatType.Card;
            ArticleListFontSize = FontSize.Large;
            ArticleListFontName = "Segoe WP SemiLight";
            
            // DEFAULT READER SETTINGS
            IsHideAppBarOnArticleViewerPageEnabled = false;
            CurrentTheme = "Day";
            ArticleViewFontSize = FontSize.Medium;
            ArticleViewFontName = "Segoe WP";

            IsSystemTrayVisibleWhenPossible = false;
            RunHistory = new Services.MostViewedHistory.RunHistory();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
