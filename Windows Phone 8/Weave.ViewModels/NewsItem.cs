using System;
using System.ComponentModel;
using Weave.ViewModels.Extensions;

namespace Weave.ViewModels
{
    public class NewsItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; }
        public Feed Feed { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string UtcPublishDateTime { get; set; }
        public string ImageUrl { get; set; }
        public string YoutubeId { get; set; }
        public string VideoUri { get; set; }
        public string PodcastUri { get; set; }
        public string ZuneAppId { get; set; }
        public DateTime OriginalDownloadDateTime { get; set; }
        public Image Image { get; set; }


        public DateTime LocalDateTime
        {
            get { return DateTime.Now; }
            set
            {

            }
        }

        public DateTime UniversalDateTime
        {
            get { return DateTime.UtcNow; }
            set
            {

            }
        }


        public bool IsFavorite
        {
            get { return isFavorite; }
            set
            {
                isFavorite = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayState"));
            }
        }
        bool isFavorite;

        public bool HasBeenViewed
        {
            get { return hasBeenViewed; }
            set
            {
                hasBeenViewed = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayState"));
            }
        }
        bool hasBeenViewed;

        public enum ColoringDisplayState
        {
            Normal,
            Viewed,
            Favorited
        }

        public ColoringDisplayState DisplayState
        {
            get
            {
                if (IsFavorite)
                    return ColoringDisplayState.Favorited;
                else
                    return HasBeenViewed ? ColoringDisplayState.Viewed : ColoringDisplayState.Normal;
            }
        }

        public double SortRating
        {
            get { return CalculateSortRating(UniversalDateTime); }
        }

        static double CalculateSortRating(DateTime dateTime)
        {
            double elapsedHours = (DateTime.UtcNow - dateTime).TotalHours;
            if (elapsedHours <= 0)
                elapsedHours = 0.0001;
            double value = 1d / elapsedHours;
            return value;
        }

        public string OriginalSource
        {
            get { return Feed.Name; }
        }

        public string OriginalFeedUri
        {
            get { return Feed.Uri; }
        }

        public string PublishDate
        {
            get { return LocalDateTime.ElapsedTime(); }
        }

        public string FormattedForMainPageSourceAndDate
        {
            get
            {
                return string.Format("{0} • {1}", Feed.Name, LocalDateTime.ElapsedTime());
            }
        }

        public bool HasImage
        {
            get
            {
                return !string.IsNullOrEmpty(ImageUrl) &&
                    Uri.IsWellFormedUriString(ImageUrl, UriKind.Absolute);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}   from {2}", Feed.Category, Title, OriginalFeedUri);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
