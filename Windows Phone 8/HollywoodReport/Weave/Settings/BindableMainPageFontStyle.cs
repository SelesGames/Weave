using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace weave
{
    public class BindableMainPageFontStyle : INotifyPropertyChanged
    {
        PermanentState permstate;
        FontSizes fontSizes;

        public BindableMainPageFontStyle()
        {
            permstate = AppSettings.Instance.PermanentState.Get().WaitOnResult();
            fontSizes = new FontSizes();

            Observable.FromEventPattern<PropertyChangedEventArgs>(permstate, "PropertyChanged").Select(o => o.EventArgs.PropertyName).Subscribe(property =>
            {
                if (property == "MainPageArticleListFontSize")
                {
                    RefreshCurrentFontSizeAttributes();
                }

                else if (property == "MainPageArticleListFontThickness")
                {
                    this.FontThickness = permstate.MainPageArticleListFontThickness.ToFontFamily();
                    PropertyChanged.Raise(this, "FontThickness");
                }
            });

            RefreshCurrentFontSizeAttributes();

            this.FontThickness = permstate.MainPageArticleListFontThickness.ToFontFamily();

            this.TitleSizeBinding = new Binding("FontTitleSize") { Source = this };
            this.DescriptionSizeBinding = new Binding("FontDescriptionSize") { Source = this };
            this.LineHeightBinding = new Binding("FontLineHeight") { Source = this };
            this.PublicationLineSizeBinding = new Binding("FontPublicationLineSize") { Source = this };
            this.ThicknessBinding = new Binding("FontThickness") { Source = this };
            this.MainPageNewsItemMarginBinding = new Binding("MainPageNewsItemMargin") { Source = this };
        }

        void RefreshCurrentFontSizeAttributes()
        {
            var fontSet = fontSizes.SingleOrDefault(o => o.Id == permstate.MainPageArticleListFontSize);
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

        public double FontTitleSize { get; set; }
        public double FontDescriptionSize { get; set; }
        public double FontLineHeight { get; set; }
        public double FontPublicationLineSize { get; set; }
        public FontFamily FontThickness { get; set; }
        public Thickness MainPageNewsItemMargin { get; set; }


        internal Binding TitleSizeBinding { get; private set; }
        internal Binding DescriptionSizeBinding { get; private set; }
        internal Binding LineHeightBinding { get; private set; }
        internal Binding PublicationLineSizeBinding { get; private set; }
        internal Binding ThicknessBinding { get; private set; }
        internal Binding MainPageNewsItemMarginBinding { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
