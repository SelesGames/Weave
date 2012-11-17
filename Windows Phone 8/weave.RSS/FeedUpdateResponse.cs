using System.Collections.Generic;

namespace Weave.RSS
{
    public class FeedUpdateResponse
    {
        public bool IsUnchanged { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; set; }
        public IEnumerable<NewsItem> News { get; set; }
    }
}
