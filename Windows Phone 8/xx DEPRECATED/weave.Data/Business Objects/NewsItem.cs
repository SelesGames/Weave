using ProtoBuf;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Weave.Data
{
    [DataContract]
    [ProtoContract]
    public class NewsItem : INotifyPropertyChanged//, INewsItem
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

        public FeedSource FeedSource { get; set; }

        public string OriginalSource
        {
            get { return FeedSource.FeedName; }
        }

        public string OriginalFeedUri
        {
            get { return FeedSource.FeedUri; }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}   from {2}", FeedSource.Category, Title, OriginalFeedUri);
        }
    
        public event PropertyChangedEventHandler  PropertyChanged;
    }
}