using System;
using System.ComponentModel;
using Weave.Customizability;
using Weave.SavedState.MostViewedHistory;

namespace Weave.SavedState
{
    public class PermanentState : INotifyPropertyChanged
    {
        public bool IsFirstTime { get; set; }
        public Guid UserId { get; set; }
        public bool IsHideAppBarOnArticleListPageEnabled { get; set; }
        public bool IsHideAppBarOnArticleViewerPageEnabled { get; set; }
        public bool IsSystemTrayVisibleWhenPossible { get; set; }
        public RunHistory RunHistory { get; set; }
        public string SpeakTextVoice { get; set; }




        #region Propertes: settings that tweak the MainPage (aka Article List) view

        public ArticleListFormatType ArticleListFormat
        {
            get { return this.articleListFormat; }
            set
            {
                if (this.articleListFormat != value)
                {
                    this.articleListFormat = value;
                    Raise("ArticleListFormat");
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
                    Raise("ArticleListFontSize");
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
                    Raise("ArticleListFontName");
                }
            }
        }
        string articleListFontName;

        #endregion




        #region Properties: settings that tweak the Article View

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
            RunHistory = new RunHistory();
        }




        #region INotifyPropertyChanged related

        void Raise(params string[] p)
        {
            if (p == null)
                return;

            if (PropertyChanged != null)
            {
                foreach (var property in p)
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
