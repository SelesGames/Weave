using System.Windows;
using System.Windows.Controls;

namespace weave
{
	public partial class NewsItemWithImageTile2 : UserControl
	{
        public NewsItemWithImageTile2()
		{
			InitializeComponent();

            if (this.IsInDesignMode())
                return;

            //headlineContainer.Opacity = 0;
            //this.ClipToSize();

            //headlineContainer.SizeChanged += (s, e) =>
            //{
            //    double translationAmount = e.NewSize.Height;
            //    HeadlineSlide.From = translationAmount;
            //    headlineContainer.Opacity = 1;
            //    OnLoadSB.Begin();
            //};
		}

        void OnClick(object sender, RoutedEventArgs e)
        {
            var newsItem = DataContext as NewsItem;
            if (newsItem == null)
                return;

            var panorama = SamplePanorama.Current;
            panorama.isPopupShowing = true;
            panorama.detailedArticleViewer.Show(newsItem);
        }
    }
}