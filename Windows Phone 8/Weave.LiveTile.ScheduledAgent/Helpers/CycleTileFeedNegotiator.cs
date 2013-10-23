using Microsoft.Phone.Shell;
using SelesGames.Common.Hashing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.LiveTile.ScheduledAgent.ViewModels;
using Weave.User.Service.Contracts;
using Weave.User.Service.DTOs;

namespace Weave.LiveTile.ScheduledAgent
{
    public class CycleTileFeedNegotiator : TileNegotiatorBase
    {
        Guid userId;
        Guid feedId;
        string appName;
        IWeaveUserService serviceClient;

        public CycleTileFeedNegotiator(
            Guid userId, 
            IWeaveUserService serviceClient, 
            Guid feedId, 
            string appName, 
            ShellTile tile)

            : base(appName, tile)
        {
            this.userId = userId;
            this.serviceClient = serviceClient;
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
            var news = await serviceClient.GetNews(userId, feedId, take: 15, type: NewsItemType.New, requireImage: true);

            var imagePrefix = CreateImagePrefix();
            var imageUrls = await news.News.Select(o => o.ImageUrl).CreateImageUrisFromNews(imagePrefix, TimeSpan.FromSeconds(15));
            Uri preferredLockScreen = null;
            var attempt = await new LockScreenSavingClient().TryGetLocalStorageUri(imageUrls.First());
            if (attempt.Item1)
                preferredLockScreen = attempt.Item2;

            var feedName = news.Feeds.FirstOrDefault().Name.ToTitleCase();

            Trace.Output("image downloads complete");

            ViewModel = new CycleTileViewModel
            {
                ImageIsoStorageUris = imageUrls,
                NewCount = news.NewArticleCount,
                RecommendedLockScreenImageUri = preferredLockScreen,
                AppName = appName + " " + feedName,
            };
        }
    }
}
