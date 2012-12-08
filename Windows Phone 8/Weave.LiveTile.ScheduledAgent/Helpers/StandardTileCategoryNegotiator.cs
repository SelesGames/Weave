using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Threading.Tasks;
using weave;
using weave.Data;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace Weave.LiveTile.ScheduledAgent
{
    public class StandardTileCategoryNegotiator : TileNegotiatorBase
    {
        string categoryName;

        public StandardTileCategoryNegotiator(string categoryName, string appName, ShellTile tile)
            : base(appName, tile)
        {
            this.categoryName = categoryName;
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
            
            var latestNewsArticle = news.Where(o => o.HasImage).FirstOrDefault();
            if (latestNewsArticle == null)
                return;

            Trace.Output("feed refresh complete");

            var image = await base.GetImageAsync(latestNewsArticle.ImageUrl);

            Trace.Output("image download complete");

            ViewModel = new StandardTileViewModel
            {
                Category = categoryName,
                Headline = latestNewsArticle.Title,
                ImageUrl = latestNewsArticle.ImageUrl,
                NewCount = string.Format("{0} NEW", news.Count),
                Source = image,
            };
        }
    }
}
