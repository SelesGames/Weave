using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;

namespace weave
{
    public partial class StarredNewsItemUserControl : UserControl
    {
        Subject<StarredNewsItem> newsItemSelected = new Subject<StarredNewsItem>();
        public IObservable<StarredNewsItem> NewsItemSelected { get { return newsItemSelected.AsObservable(); } }

        public StarredNewsItemUserControl()
        {
            InitializeComponent();
        }

        internal void SetNewsItem(StarredNewsItem newsItem)
        {
            title.Text = newsItem.Title;
            publishedDate.Text = newsItem.FormattedForMainPageSourceAndDate;
            //image.ImageSource = newsItem.HasImage ? ImageCache.GetImage(newsItem.ImageUrl) : null;

            //Binding b = new Binding("HasBeenViewed")
            //{
            //    Converter = new DelegateValueConverter(value =>
            //        {
            //            var hasBeenViewed = (bool)value;
            //            return hasBeenViewed ? 0.5d : 1d;
            //        },
            //        null),
            //    Source = newsItem
            //};
            //title.SetBinding(UIElement.OpacityProperty, b);
            //publishedDate.SetBinding(UIElement.OpacityProperty, b);
            //if (newsItem.HasImage)
            //    imageRect.SetBinding(UIElement.OpacityProperty, b);
            //sideBar.SetBinding(UIElement.OpacityProperty, b);

            imageRect.Visibility = newsItem.HasImage ? Visibility.Visible : Visibility.Collapsed;
            //newsItem.SanitizeDescription();
            //description.Text = newsItem.Description;
            this.Tag = newsItem;
        }

        void newsItemClicked(object sender, RoutedEventArgs e)
        {
            newsItemSelected.OnNext(this.Tag as StarredNewsItem);
        }

        public ImageCache ImageCache { get; set; }
    }
}
