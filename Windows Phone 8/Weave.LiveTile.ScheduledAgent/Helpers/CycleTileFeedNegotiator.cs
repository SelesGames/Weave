using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Threading.Tasks;
using weave.Data;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace Weave.LiveTile.ScheduledAgent
{
    public class CycleTileFeedNegotiator : TileNegotiatorBase
    {
        Guid feedId;

        public CycleTileFeedNegotiator(Guid feedId, string appName, ShellTile tile) 
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

            var imageUrls = await news.CreateImageUrisFromNews(TimeSpan.FromSeconds(15));
            Uri preferredLockScreen = null;
            var attempt = await new LockScreenSavingClient().TryGetLocalStorageUri(imageUrls.First());
            if (attempt.Item1)
                preferredLockScreen = attempt.Item2;

            Trace.Output("image downloads complete");

            ViewModel = new CycleTileViewModel
            {
                ImageIsoStorageUris = imageUrls,
                NewCount = news.Count,
                RecommendedLockScreenImageUri = preferredLockScreen,
            };
        }
    }
}
