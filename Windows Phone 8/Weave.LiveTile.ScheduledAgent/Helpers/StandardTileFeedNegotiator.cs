using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Threading.Tasks;
using weave.Data;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace Weave.LiveTile.ScheduledAgent
{
    public class StandardTileFeedNegotiator : TileNegotiatorBase
    {
        Guid feedId;

        public StandardTileFeedNegotiator(Guid feedId, string appName, ShellTile tile) 
            : base(appName, tile)
        {
            this.feedId = feedId;
        }

        protected async override Task InitializeViewModelAsync()
        {
            if (feedId == null)
                return;

            var dal = new Weave4DataAccessLayer();
            var feeds = await dal.GetFeedsAsync();

            var feed = feeds.FirstOrDefault(o => feedId.Equals(o.Id));
            if (feed == null)
                return;

            Trace.Output("refreshing feed: " + feed.FeedName);

            feed.RefreshNews();
            await feed.CurrentRefresh;

            if (feed.News == null || !feed.News.Any())
                return;

            var news = feed.News.OrderByDescending(o => o.PublishDateTime).ToList();

            var latestNewsArticle = news.Where(o => o.HasImage).FirstOrDefault();
            if (latestNewsArticle == null)
                return;

            Trace.Output("feed refresh complete");

            var image = await base.GetImageAsync(latestNewsArticle.ImageUrl);

            Trace.Output("image download complete");

            ViewModel = new StandardTileViewModel
            {
                Category = feed.FeedName,
                Headline = latestNewsArticle.Title,
                ImageUrl = latestNewsArticle.ImageUrl,
                NewCount = string.Format("{0} NEW", news.Count),
                Source = image,
            };

            Trace.Output("viewmodel created");
        }
    }
}
