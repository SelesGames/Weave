using System;
using System.ComponentModel;
using Weave.Customizability;

namespace weave
{
    public class PermanentState : INotifyPropertyChanged
    {
        public bool IsFirstTime { get; set; }
        public UserInfo User { get; set; }
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

        public ArticleListFormatType ArticleListFormat { get; set; }

        public FontSize MainPageArticleListFontSize
        {
            get { return this.mainPageArticleListFontSize; }
            set
            {
                if (this.mainPageArticleListFontSize != value)
                {
                    this.mainPageArticleListFontSize = value;
                    PropertyChanged.Raise(this, "MainPageArticleListFontSize");
                }
            }
        }
        FontSize mainPageArticleListFontSize;

        public FontThickness MainPageArticleListFontThickness
        {
            get { return this.mainPageArticleListFontThickness; }
            set
            {
                if (this.mainPageArticleListFontThickness != value)
                {
                    this.mainPageArticleListFontThickness = value;
                    PropertyChanged.Raise(this, "MainPageArticleListFontThickness");
                }
            }
        }
        FontThickness mainPageArticleListFontThickness;

        #endregion




        #region Settings that tweak the Article View

        public string CurrentTheme { get; set; }
        public FontSize ArticleViewFontSize { get; set; }
        public string ArticleViewFontName { get; set; }

        #endregion




        public PermanentState()
        {
            IsFirstTime = true; // start off as true
            ArticleListFormat = ArticleListFormatType.BigImage;
            mainPageArticleListFontSize = FontSize.MediumLarge;
            mainPageArticleListFontThickness = FontThickness.Regular;
            CurrentTheme = "Day";
            ArticleViewFontSize = FontSize.Medium;
            ArticleViewFontName = "Segoe WP";
            IsHideAppBarOnArticleListPageEnabled = false;
            IsHideAppBarOnArticleViewerPageEnabled = false;
            IsSystemTrayVisibleWhenPossible = false;
            RunHistory = new Services.MostViewedHistory.RunHistory();
            User = UserInfo.CreateNewUser();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
