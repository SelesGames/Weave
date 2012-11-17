using System.Windows;
using System.Windows.Controls;

namespace weave
{
	public partial class NewsItemTile : UserControl
	{
        string headline;
        NewsItem newsItem;

		public NewsItemTile()
		{
			InitializeComponent();
		}

        void OnClick(object sender, RoutedEventArgs e)
        {
            var panorama = SamplePanorama.Current;
            panorama.isPopupShowing = true;
            panorama.detailedArticleViewer.Show(newsItem);
        }

        public string Headline
        {
            get { return headline; }
            set
            {
                headline = value;
                headlineTxt.Text = value;
            }
        }

        public NewsItem NewsItem
        {
            get { return newsItem; }
            set
            {
                newsItem = value;
                Headline = value.Title;
            }
        }
	}
}