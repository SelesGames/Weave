using ProtoBuf;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace weave
{
    [DataContract]
    [ProtoContract]
    public class NewsItem : INotifyPropertyChanged, INewsItem
    {
        [DataMember][ProtoMember(1)]    public Guid FeedId { get; set; }
        [DataMember][ProtoMember(2)]    public string Title { get; set; }
        [DataMember][ProtoMember(3)]    public string Link { get; set; }
        [DataMember][ProtoMember(4)]    public string ImageUrl { get; set; }
        [DataMember][ProtoMember(5)]    public string YoutubeId { get; set; }
        [DataMember][ProtoMember(6)]    public string VideoUri { get; set; } 
        [DataMember][ProtoMember(7)]    public string PodcastUri { get; set; }
        [DataMember][ProtoMember(8)]    public string ZuneAppId { get; set; }
        [DataMember][ProtoMember(9)]    public DateTime PublishDateTime { get; set; }
        [DataMember][ProtoMember(10)]   public DateTime OriginalDownloadDateTime { get; set; }
        [DataMember][ProtoMember(11)]   public bool IsFavorite
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
        [DataMember][ProtoMember(12)]   public bool HasBeenViewed 
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

        public FeedSource FeedSource { get; set; }

        public double SortRating 
        {
            get { return CalculateSortRating(PublishDateTime); } 
        }

        static double CalculateSortRating(DateTime dateTime)
        {
            double elapsedHours = (DateTime.Now - dateTime).TotalHours;
            if (elapsedHours <= 0)
                elapsedHours = 0.0001;
            double value = 1d / elapsedHours;
            return value;
        }

        public string OriginalSource
        {
            get { return FeedSource.FeedName; }
        }

        public string OriginalFeedUri
        {
            get { return FeedSource.FeedUri; }
        }

        public string PublishDate
        {
            get { return PublishDateTime.ElapsedTime(); }
        }

        public string FormattedForMainPageSourceAndDate
        {
            get
            {
                return string.Format("{0} • {1}", FeedSource.FeedName, PublishDateTime.ElapsedTime());
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
            return string.Format("{0}: {1}   from {2}", FeedSource.Category, Title, OriginalFeedUri);
        }
    
        public event PropertyChangedEventHandler  PropertyChanged;
    }
}
