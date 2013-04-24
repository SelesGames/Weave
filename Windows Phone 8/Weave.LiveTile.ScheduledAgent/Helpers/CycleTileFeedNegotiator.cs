using Microsoft.Phone.Shell;
using SelesGames.Common.Hashing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.LiveTile.ScheduledAgent.ViewModels;
using Weave.ViewModels.Contracts.Client;

namespace Weave.LiveTile.ScheduledAgent
{
    public class CycleTileFeedNegotiator : TileNegotiatorBase
    {
        Guid feedId;
        string appName;
        IViewModelRepository serviceClient;

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
            var news = await serviceClient.GetFeaturedNews(feedId, 15, refresh: true);

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
                NewCount = news.Count(),
                RecommendedLockScreenImageUri = preferredLockScreen,
                AppName = appName + " " + "temp",//feedName.ToTitleCase(),
            };
        }
    }
}
