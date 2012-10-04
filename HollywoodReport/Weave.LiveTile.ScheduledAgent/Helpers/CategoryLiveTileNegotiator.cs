using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using weave;
using weave.Data;

namespace Weave.LiveTile.ScheduledAgent
{
    public class CategoryLiveTileNegotiator : LiveTileNegotiatorBase
    {
        string categoryName;

        public CategoryLiveTileNegotiator(string categoryName, ShellTile tile)
            : base(tile)
        {
            this.categoryName = categoryName;
        }

        protected async override Task InitializeViewModelAsync()
        {
            if (string.IsNullOrEmpty(categoryName))
                return;

            var dal = new Weave4DataAccessLayer();
            var feeds = await dal.GetFeedsAsync();

            if (!categoryName.Equals("all news", StringComparison.OrdinalIgnoreCase))
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

            ViewModel = new LiveTileViewModel
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
