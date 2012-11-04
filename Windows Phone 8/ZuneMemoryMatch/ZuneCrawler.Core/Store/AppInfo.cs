using System;

namespace ZuneCrawler.Core
{
    public class AppInfo
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        //public DateTime Updated { get; set; }
        public string Version { get; set; }
        public string ShortDescription { get; set; }
        public decimal AverageUserRating { get; set; }
        public int UserRatingCount { get; set; }
        public string ImageId { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public string OfferType { get; set; } // Free, Paid, Trial
        public decimal Price { get; set; }
        public bool IsXboxLive { get; set; }
        //public string BackgroundImageId { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
