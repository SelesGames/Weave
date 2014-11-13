using Microsoft.Phone.Shell;
using Common.Hashing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.LiveTile.ScheduledAgent.ViewModels;
using Weave.Services.User.Contracts;
using Weave.Services.User.DTOs;

namespace Weave.LiveTile.ScheduledAgent
{
    public class CycleTileCategoryNegotiator : TileNegotiatorBase
    {
        Guid userId;
        string categoryName;
        string appName;
        string tileTitle;
        IWeaveUserService serviceClient;

        public CycleTileCategoryNegotiator(
            Guid userId, 
            IWeaveUserService serviceClient, 
            string categoryName, 
            string appName, 
            ShellTile tile)

            : base(appName, tile)
        {
            this.userId = userId;
            this.serviceClient = serviceClient;
            this.categoryName = System.Net.HttpUtility.UrlDecode(categoryName);
            this.appName = appName;

            if (categoryName == null || categoryName.Equals("all news"))
            {
                this.tileTitle = appName;
            }
            else
            { 
                this.tileTitle = appName + " " + this.categoryName.ToTitleCase();
            }
        }

        string CreateImagePrefix()
        {
            var appNameWithCategory = string.Format("{0}{1}", appName, categoryName);
            var hashed = CryptoHelper.ComputeHash(appNameWithCategory);
            return string.Format("{0}.photo", hashed);
        }

        protected async override Task InitializeViewModelAsync()
        {
            var news = await serviceClient.GetNews(userId, categoryName, take: 15, type: NewsItemType.New, requireImage: true);

            var imagePrefix = CreateImagePrefix();
            var imageUrls = await news.News.Select(o => o.ImageUrl).CreateImageUrisFromNews(imagePrefix, TimeSpan.FromSeconds(15));
            Uri preferredLockScreen = null;
            var attempt = await new LockScreenSavingClient().TryGetLocalStorageUri(imageUrls.First());
            if (attempt.Item1)
                preferredLockScreen = attempt.Item2;

            Trace.Output("image downloads complete");

            ViewModel = new CycleTileViewModel
            {
                ImageIsoStorageUris = imageUrls,
                NewCount = news.NewArticleCount,
                RecommendedLockScreenImageUri = preferredLockScreen,
                AppName = tileTitle,
            };
        }
    }
}