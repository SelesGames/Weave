using Microsoft.Phone.Shell;
using SelesGames.Common.Hashing;
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
        string appName;

        public CycleTileFeedNegotiator(Guid feedId, string appName, ShellTile tile) 
            : base(appName, tile)
        {
            this.appName = appName;
            this.feedId = feedId;
        }

        string CreateImagePrefix()
        {
            var appNameWithCategory = string.Format("{0}{1}", appName, feedId);
            var hashed = CryptoHelper.ComputeHash(appNameWithCategory);
            return string.Format("{0}.photo", hashed);
        }

        protected async override Task InitializeViewModelAsync()
        {
            string feedName;

            if (feedId == null)
                return;

            var dal = new Weave4DataAccessLayer();
            var feeds = await dal.GetFeedsAsync();

            var feed = feeds.FirstOrDefault(o => feedId.Equals(o.Id));
            if (feed == null)
                return;

            feedName = feed.FeedName;

            Trace.Output("refreshing feed: " + feed.FeedName);

#if DEBUG
            feed.ResetFeed();
#endif
            feed.RefreshNews();

            await feed.CurrentRefresh;

            if (feed.News == null || !feed.News.Any())
                return;

            var news = feed.News.OrderByDescending(o => o.PublishDateTime).ToList();

            var imagePrefix = CreateImagePrefix();
            var imageUrls = await news.CreateImageUrisFromNews(imagePrefix, TimeSpan.FromSeconds(15));
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
                AppName = appName + " " + feedName.ToTitleCase(),
            };
        }
    }
}
