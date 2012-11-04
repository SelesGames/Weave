using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace weave
{
    public partial class StarredBigImageControl : UserControl
    {
        IDisposable disp;

        static readonly BitmapImage failImage =
            new BitmapImage(new Uri("/weave;component/Assets/imageDownloadFailed.png", UriKind.Relative));

        public ImageCache ImageCache { get; set; }


        public StarredBigImageControl()
        {
            // Required to initialize variables
            InitializeComponent();
            ImageCache = Page3.imageCache;
        }

        public void SetStarredNewsItem(StarredNewsItem newsItem)
        {
            ImageFadeInSB.Stop();
            if (disp != null)
                disp.Dispose();

            title.Text = newsItem.Title;
            source.Text = newsItem.FeedName;

            if (newsItem.HasImage)
            {
                imageRect.Opacity = 0;
                var imageAndNotification = ImageCache.GetImageWithNotification(newsItem.ImageUrl);
                if (imageAndNotification != null)
                {
                    image.ImageSource = imageAndNotification.Item1;
                    disp = imageAndNotification.Item2
                        .ObserveOnDispatcher()
                        .Subscribe(o =>
                            this.ImageFadeInSB.Begin(),
                            ex =>
                            {
                                image.ImageSource = failImage;// new BitmapImage(new Uri("/weave;component/Assets/imageDownloadFailed.png", UriKind.Relative));
                                this.ImageFadeInSB.Begin();
                            });
                }
                else
                    image.ImageSource = null;
            }
            else
            {
                image.ImageSource = null;
            }

            imageRect.Visibility = newsItem.HasImage ? Visibility.Visible : Visibility.Collapsed;
            this.Tag = newsItem;
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data",
            typeof(StarredNewsItem),
            typeof(StarredBigImageControl), 
            new PropertyMetadata(null, OnDataChanged));

        public StarredNewsItem Data
        {
            get { return (StarredNewsItem)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StarredBigImageControl c = d as StarredBigImageControl;
            if (e.NewValue != null && e.NewValue != e.OldValue)
            {
                c.SetStarredNewsItem(e.NewValue as StarredNewsItem);
            }
        }
    }
}