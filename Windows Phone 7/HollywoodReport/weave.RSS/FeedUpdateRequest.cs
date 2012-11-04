using System;

namespace weave.Services.RSS
{
    public class FeedUpdateRequest
    {
        public string FeedUri { get; set; }
        public string Etag { get; set; }
        public DateTime LastRefreshedOn { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string LastModified { get; set; }
    }
}
