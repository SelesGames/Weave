using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using ProtoBuf;

namespace weave
{
    [DataContract]
    [ProtoContract]
    public class NewsItem : INotifyPropertyChanged, INewsItem
    {
        bool hasBeenViewed;

        [DataMember][ProtoMember(1)] public string Title { get; set; }
        [DataMember][ProtoMember(2)] public string Link { get; set; }
        [DataMember][ProtoMember(3)] public string Description { get; set; }
        [DataMember][ProtoMember(4)] public DateTime PublishDateTime { get; set; }
        [DataMember][ProtoMember(5)] public string ImageUrl { get; set; }
        [DataMember][ProtoMember(6)] public bool HasBeenViewed 
        {
            get { return hasBeenViewed; }
            set { hasBeenViewed = value; PropertyChanged.Raise(this, "HasBeenViewed"); } 
        }

        public bool IsNew { get; set; }

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

        public string FormattedForPopupsSourceAndDate
        {
            get 
            { 
                return PublishDateTime.ToString("MMMM dd, h:mm tt") + 
                    " via " + FeedSource.FeedName;
            }
        }

        public string FormattedForMainPageSourceAndDate
        {
            get
            {
                return PublishDateTime.ElapsedTime();
                //return PublishDateTime.ElapsedTime() + " via " + FeedSource.FeedName;
                //return string.Format(
                //    "{0}: {1}",
                //    FeedSource.FeedName,
                //    PublishDateTime.ElapsedTime());
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
