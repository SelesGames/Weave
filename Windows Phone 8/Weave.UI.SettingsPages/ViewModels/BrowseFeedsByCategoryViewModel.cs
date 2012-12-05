using SelesGames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using weave.Data;
using Weave.FeedLibrary;

namespace weave
{
    public class BrowseFeedsByCategoryViewModel
    {
        string category;
        List<FeedSource> existingFeeds;
        List<Feed> snapshotOfEnabledFeeds;
        Weave4DataAccessLayer dataRepo;




        #region internal classes for formatting/displaying page data

        public class Feed
        {
            public string Name { get; set; }
            public string Image { get; set; }
            public string Uri { get; set; }
            public bool IsEnabled { get; set; }
            public ArticleViewingType ViewType { get; set; }
        }

        #endregion



        public string Category
        {
            get { return category.ToLower(); }
        }

        public ObservableCollection<Feed> Feeds { get; private set; }

        public BrowseFeedsByCategoryViewModel(string category)
        {
            this.category = category;
            Feeds = new ObservableCollection<Feed>();
            dataRepo = ServiceResolver.Get<weave.Data.Weave4DataAccessLayer>();
        }

        public async Task LoadFeedsAsync()
        {
            Feeds.Clear();

            existingFeeds = await dataRepo.Feeds.Get();

            var library = ServiceResolver.Get<ExpandedLibrary>();
            var temp = await library.Feeds.Get();
            var feeds = temp           
                .OfCategory(category)
                .OrderBy(o => o.FeedName)
                .Select(Parse)
                .OfType<Feed>()
                .ToList();

            foreach (var feed in feeds)
            {
                Feeds.Add(feed);
            }

            snapshotOfEnabledFeeds = Feeds.Where(o => o.IsEnabled).ToList();
        }

        Feed Parse(FeedSource feed)
        {
            if (string.IsNullOrEmpty(feed.FeedUri))
                return null;

            return new Feed
            {
                Name = feed.FeedName,
                Image = null,
                Uri = feed.FeedUri,
                IsEnabled = existingFeeds.Any(o => o.FeedUri.Equals(feed.FeedUri, StringComparison.OrdinalIgnoreCase)),
                ViewType = feed.ArticleViewingType,
            };
        }

        public async Task SaveChanges()
        {
            var currentlyEnabledFeeds = Feeds.Where(o => o.IsEnabled).ToList();
            var newlyEnabledFeeds = currentlyEnabledFeeds.Except(snapshotOfEnabledFeeds).ToList();
            var newlyDisabledFeeds = snapshotOfEnabledFeeds.Except(currentlyEnabledFeeds).ToList();

            if (newlyEnabledFeeds.Count == 0 && newlyDisabledFeeds.Count == 0)
                return;

            var newlyDisabledFeedSources = newlyDisabledFeeds
                .Select(o => existingFeeds.FirstOrDefault(x => x.FeedUri.Equals(o.Uri, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var feed in newlyEnabledFeeds)
            {
                await dataRepo.AddCustomFeed(new FeedSource
                {
                    FeedName = feed.Name,
                    FeedUri = feed.Uri,
                    Category = category,
                    ArticleViewingType = feed.ViewType,
                });
            }

            foreach (var feed in newlyDisabledFeedSources)
            {
                await dataRepo.DeleteFeed(feed);
            }

            await dataRepo.SaveFeeds();
        }
    }
}