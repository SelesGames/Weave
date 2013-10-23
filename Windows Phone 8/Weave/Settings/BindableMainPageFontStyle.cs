using SelesGames;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Weave.SavedState;

namespace weave
{
    public class BindableMainPageFontStyle : INotifyPropertyChanged
    {
        PermanentState permstate;
        FontSizes fontSizes;
        FontSet fontSet;

        public BindableMainPageFontStyle()
        {
            permstate = ServiceResolver.Get<PermanentState>();
            fontSizes = new FontSizes();
            fontSet = new ArticleListFontSet();

            Observable.FromEventPattern<PropertyChangedEventArgs>(permstate, "PropertyChanged").Select(o => o.EventArgs.PropertyName).Subscribe(property =>
            {
                if (property == "ArticleListFontSize")
                {
                    RefreshCurrentFontSizeAttributes();
                }

                else if (property == "ArticleListFontName")
                {
                    RefreshCurrentFontFamily();
                }

                else if (property == "ArticleListFormat")
                {
                    RefreshNewsItemStyle();
                }
            });

            RefreshCurrentFontSizeAttributes();

            var font = fontSet.GetByFontName(permstate.ArticleListFontName);
            this.FontFamily = font.FontFamily;

            this.FontFamilyBinding = new Binding("FontFamily") { Source = this };
            this.TitleSizeBinding = new Binding("FontTitleSize") { Source = this };
            this.DescriptionSizeBinding = new Binding("FontDescriptionSize") { Source = this };
            this.LineHeightBinding = new Binding("FontLineHeight") { Source = this };
            this.PublicationLineSizeBinding = new Binding("FontPublicationLineSize") { Source = this };
            this.MainPageNewsItemMarginBinding = new Binding("MainPageNewsItemMargin") { Source = this };
            this.NewsItemStyleBinding = new Binding("NewsItemStyle") { Source = this };
        }




        #region when the Article List Font Size changes, propogate those changes to various size settings

        void RefreshCurrentFontSizeAttributes()
        {
            var fontSet = fontSizes.SingleOrDefault(o => o.Id == permstate.ArticleListFontSize);
            if (fontSet == null)
                return;

            this.FontTitleSize = fontSet.TitleSize;
            this.FontDescriptionSize = fontSet.DescriptionSize;
            this.FontLineHeight = fontSet.LineHeight;
            this.FontPublicationLineSize = fontSet.PublicationLineSize;
            this.MainPageNewsItemMargin = fontSet.MainPageNewsItemMargin;

            PropertyChanged.Raise(this, "FontTitleSize");
            PropertyChanged.Raise(this, "FontDescriptionSize");
            PropertyChanged.Raise(this, "FontLineHeight");
            PropertyChanged.Raise(this, "FontPublicationLineSize");
            PropertyChanged.Raise(this, "MainPageNewsItemMargin");
        }

        #endregion




        #region When Article List Font Family changes, propagate change to the Binding

        void RefreshCurrentFontFamily()
        {
            var font = fontSet.GetByFontName(permstate.ArticleListFontName);
            this.FontFamily = font.FontFamily;
            PropertyChanged.Raise(this, "FontFamily");
        }

        #endregion




        #region When News Item Style changes, propogate change to the Binding

        void RefreshNewsItemStyle()
        {

        }

        #endregion


        public FontFamily FontFamily { get; set; }
        internal Binding FontFamilyBinding { get; private set; }


        public double FontTitleSize { get; set; }
        public double FontDescriptionSize { get; set; }
        public double FontLineHeight { get; set; }
        public double FontPublicationLineSize { get; set; }
        public Thickness MainPageNewsItemMargin { get; set; }

        internal Binding TitleSizeBinding { get; private set; }
        internal Binding DescriptionSizeBinding { get; private set; }
        internal Binding LineHeightBinding { get; private set; }
        internal Binding PublicationLineSizeBinding { get; private set; }
        internal Binding MainPageNewsItemMarginBinding { get; private set; }


        public Style NewsItemStyle { get; set; }
        internal Binding NewsItemStyleBinding { get; private set; }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
