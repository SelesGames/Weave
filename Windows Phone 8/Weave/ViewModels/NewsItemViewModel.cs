using System.Windows;
using System.Windows.Media;
using Weave.ViewModels;

namespace weave
{
    public class NewsItemViewModel
    {
        public NewsItem NewsItem { get; private set; }

        public string Title { get; set; }
        public string FormattedForMainPageSourceAndDate { get; set; }
        public string ImageUrl { get; set; }
        public Brush MediaTypeImageBrush { get; set; }
        public Visibility MediaTypeVisibility
        {
            get { return MediaTypeImageBrush != null ? Visibility.Visible : Visibility.Collapsed; }
        }
        public bool HasImage { get; set; }
        public Visibility ImageVisibility
        {
            get { return HasImage ? Visibility.Visible : Visibility.Collapsed; }
        }
        public double GridWidth
        {
            get { return HasImage ? 300d : 432d; }
        }

        public NewsItemViewModel(NewsItem newsItem)
        {
            this.NewsItem = newsItem;
            Title = newsItem.Title;
            FormattedForMainPageSourceAndDate = newsItem.FormattedForMainPageSourceAndDate;
            MediaTypeImageBrush = newsItem.GetMediaTypeImageBrush();
            ImageUrl = newsItem.ImageUrl;
            HasImage = newsItem.HasImage;
        }
    }
}
