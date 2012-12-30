using Microsoft.Phone.Shell;
using SelesGames.Common.Hashing;
using System;
using System.Linq;
using System.Threading.Tasks;
using weave;
using weave.Data;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace Weave.LiveTile.ScheduledAgent
{
    public class CycleTileCategoryNegotiator : TileNegotiatorBase
    {
        string categoryName;
        string appName;
        string tileTitle;

        public CycleTileCategoryNegotiator(string categoryName, string appName, ShellTile tile)
            : base(appName, tile)
        {
            this.categoryName = System.Net.HttpUtility.UrlDecode(categoryName);
            this.appName = appName;
            this.tileTitle = categoryName == null ?
                appName : appName + " " + this.categoryName.ToTitleCase();
        }

        string CreateImagePrefix()
        {
            var appNameWithCategory = string.Format("{0}{1}", appName, categoryName);
            var hashed = CryptoHelper.ComputeHash(appNameWithCategory);
            return string.Format("{0}.photo", hashed);
        }

        protected async override Task InitializeViewModelAsync()
        {
            var dal = new Weave4DataAccessLayer();
            var feeds = await dal.GetFeedsAsync();

            if (categoryName != null && !categoryName.Equals("all news", StringComparison.OrdinalIgnoreCase))
                feeds = feeds.OfCategory(categoryName).ToList();

            Trace.Output("refreshing " + categoryName);

            FeedSource.NewsServer.BeginFeedUpdateBatch();
            foreach (var feed in feeds)
            {
#if DEBUG
                feed.ResetFeed();
#endif
                feed.RefreshNews();
            }
            FeedSource.NewsServer.EndFeedUpdateBatch();

            await feeds.Select(o => o.CurrentRefresh);

            var news = feeds.AllOrderedNews().ToList();

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
                AppName = tileTitle,
            };
        }

        //async Task<Tuple<bool, Uri>> SaveImageAndReturnUri(string imageUrl, int index)
        //{
        //    try
        //    {
        //        var image = await ImageHelper.GetImageAsync(imageUrl);
        //        var bmp = (WriteableBitmap)image;
        //        if (bmp.PixelHeight > 99 && bmp.PixelWidth > 99)
        //        {
        //            var url = bmp.SaveToIsoStorage("photo" + index);
        //            return Tuple.Create(true, url);
        //        }
        //    }
        //    catch { }
        //    return Tuple.Create(false, default(Uri));
        //}
    }
}