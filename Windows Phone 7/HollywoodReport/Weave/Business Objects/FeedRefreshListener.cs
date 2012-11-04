using System;
using System.Linq;
using System.Threading.Tasks;

namespace weave
{
    public class FeedRefreshListener : RefreshListenerBase
    {
        public Guid feedId;

        public FeedRefreshListener(Guid feedId)
        {
            this.feedId = feedId;
        }

        public async override Task GetRefreshed()
        {
            var feed = GetFeed();
            if (feed == null || feed.CurrentRefresh == null)
                return;

            await feed.CurrentRefresh;
        }

        public override int GetNewCount()
        {
            var feed = GetFeed();
            if (feed == null || feed.News == null)
                return 0;

            return feed.News.Where(o => o.IsNew()).Count();
        }

        FeedSource GetFeed()
        {
            if (feedId == null)
                return null;

            var feeds = DataAccessLayer.Feeds.Get().WaitOnResult();
            var feed = feeds.FirstOrDefault(o => o.Id.Equals(feedId));
            return feed;
        }
    }
}
