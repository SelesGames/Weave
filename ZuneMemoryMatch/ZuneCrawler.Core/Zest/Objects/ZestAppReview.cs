using System;

namespace ZuneCrawler.Core.Zest
{
    internal class ZestAppReview
    {
        public DateTime Updated { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public decimal UserRating { get; set; }
    }
}
