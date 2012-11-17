using System.Windows;
using System.Windows.Controls;

namespace weave
{
	public partial class NewsItemTile2 : UserControl
	{
		public NewsItemTile2()
		{
			InitializeComponent();
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