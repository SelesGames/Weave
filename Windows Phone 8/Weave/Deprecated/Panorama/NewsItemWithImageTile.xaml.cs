using System.Windows;
using System.Windows.Controls;

namespace weave
{
	public partial class NewsItemWithImageTile : UserControl
	{
        string imageUrl, headline;
        NewsItem newsItem;

        public NewsItemWithImageTile()
		{
			InitializeComponent();

            if (this.IsInDesignMode())
                return;

            headlineContainer.Opacity = 0; ;
            this.ClipToSize();

            headlineContainer.SizeChanged += (s, e) =>
            {
                double translationAmount = e.NewSize.Height;
                HeadlineSlide.From = translationAmount;
                headlineContainer.Opacity = 1;
                OnLoadSB.Begin();
            };

            headlineTxt.Text = null;
            image.Source = null;
            Button b = new Button();
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

        public string ImageUrl
        {
            get { return imageUrl; }
            set
            {
                imageUrl = value;
                image.Source = value;
            }
        }

        public NewsItem NewsItem
        {
            get { return newsItem; }
            set 
            {
                newsItem = value;
                ImageUrl = value.ImageUrl;
                Headline = value.Title;
            }
        }

        //public static readonly DependencyProperty ImageUrlProperty =
        //    DependencyProperty.Register(
        //        "EnableVerticalScrollbar",
        //        typeof(string), typeof(NewsItemTileWithImage),
        //        new PropertyMetadata(null, OnImageUrlPropertyChanged));

        //public string ImageUrl
        //{
        //    get { return (string)GetValue(ImageUrlProperty); }
        //    set { SetValue(ImageUrlProperty, value); }
        //}

        //protected static void OnImageUrlPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //{
        //    var me = o as NewsItemTileWithImage;
        //    if (me != null)
        //    {
        //        var url = e.NewValue.ToString();
        //        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        //            HttpWebRequest.Create(url).GetResponseStreamAsync()
        //                .ObserveOnDispatcher()
        //                .Select(i => new { x = new BitmapImage(), Stream = i })
        //                .Do(i => i.x.SetSource(i.Stream))
        //                .Subscribe(i => me.image.Source = i.x);
        //    }
        //}
    }
}