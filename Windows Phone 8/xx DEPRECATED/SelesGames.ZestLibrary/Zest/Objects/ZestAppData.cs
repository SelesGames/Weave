using System;
using System.Collections.Generic;

namespace ZuneCrawler.Core.Zest
{
    internal class ZestAppData
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime Updated { get; set; }
        public string Version { get; set; }
        public string ShortDescription { get; set; }
        public decimal AverageUserRating { get; set; }
        public int UserRatingCount { get; set; }
        public string ImageId { get; set; }

        public IList<ZestCategory> Categories = new List<ZestCategory>();
        public IList<ZestPublisher> Publisher = new List<ZestPublisher>();
        public IList<ZestOffer> Offers = new List<ZestOffer>();
        public IList<string> Tags = new List<string>();

        public override string ToString()
        {
            return Title;
        }
    }
}
