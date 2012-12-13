using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using weave;
using weave.Data;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace Weave.LiveTile.ScheduledAgent
{
    public class CycleTileCategoryNegotiator : TileNegotiatorBase
    {
        string categoryName;
        string appName;

        public CycleTileCategoryNegotiator(string categoryName, string appName, ShellTile tile)
            : base(appName, tile)
        {
            this.categoryName = categoryName;
            this.appName = appName;
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
                feed.RefreshNews();
            FeedSource.NewsServer.EndFeedUpdateBatch();

            await feeds.Select(o => o.CurrentRefresh);

            var news = feeds.AllOrderedNews().ToList();

            var imageUrls = new List<Uri>();
            var startTime = DateTime.Now;

            foreach (var newsItem in news.Where(o => o.HasImage))
            {
                var attempt = await SaveImageAndReturnUri(newsItem.ImageUrl, imageUrls.Count + 1);
                if (attempt.Item1)
                {
                    imageUrls.Add(attempt.Item2);

                    var elapsed = DateTime.Now - startTime;
                    if (imageUrls.Count >= 9 || elapsed > TimeSpan.FromSeconds(15))
                        break;
                }
            }

            if (imageUrls.Count() < 2)
                return;

            Trace.Output("image downloads complete");

            ViewModel = new CycleTileViewModel
            {
                AppName = appName,
                ImageIsoStorageUris = imageUrls.ToArray(),
                NewCount = news.Count,
            };
        }

        async Task<Tuple<bool, Uri>> SaveImageAndReturnUri(string imageUrl, int index)
        {
            try
            {
                var image = await ImageHelper.GetImageAsync(imageUrl);
                var bmp = (WriteableBitmap)image;
                if (bmp.PixelHeight > 99 && bmp.PixelWidth > 99)
                {
                    var url = bmp.SaveToIsoStorage("photo" + index);
                    return Tuple.Create(true, url);
                }
            }
            catch { }
            return Tuple.Create(false, default(Uri));
        }
    }
}
