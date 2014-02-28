using SelesGames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedLibrary;
using Weave.ViewModels;

namespace weave
{
    public class BrowseFeedsByCategoryViewModel
    {
        UserInfo user;
        string category;
        IEnumerable<Weave.ViewModels.Feed> existingFeeds;
        List<Feed> snapshotOfEnabledFeeds;




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
            user = ServiceResolver.Get<UserInfo>();
        }

        public async Task LoadFeedsAsync()
        {
            Feeds.Clear();

            existingFeeds = user.Feeds;

            var library = ServiceResolver.Get<ExpandedLibrary>();
            var temp = await library.Feeds.Value;
            var feeds = temp           
                .OfCategory(category)
                .OrderBy(o => o.Name)
                .Select(Parse)
                .OfType<Feed>()
                .ToList();

            foreach (var feed in feeds)
            {
                Feeds.Add(feed);
            }

            snapshotOfEnabledFeeds = Feeds.Where(o => o.IsEnabled).ToList();
        }

        public async Task SaveChanges()
        {
            var currentlyEnabledFeeds = Feeds.Where(o => o.IsEnabled).ToList();
            var newlyEnabledFeeds = currentlyEnabledFeeds.Except(snapshotOfEnabledFeeds).Select(Parse).OfType<Weave.ViewModels.Feed>().ToList();
            var newlyDisabledFeeds = snapshotOfEnabledFeeds.Except(currentlyEnabledFeeds).ToList();

            if (newlyEnabledFeeds.Count == 0 && newlyDisabledFeeds.Count == 0)
                return;

            var newlyDisabledFeedSources = newlyDisabledFeeds
                .Select(o => existingFeeds.FirstOrDefault(x => x.Uri.Equals(o.Uri, StringComparison.OrdinalIgnoreCase)))
                .OfType<Weave.ViewModels.Feed>()
                .ToList();

            await user.BatchChange(newlyEnabledFeeds, newlyDisabledFeedSources);
        }




        #region parse to and fro

        Feed Parse(Weave.ViewModels.Feed feed)
        {
            if (string.IsNullOrEmpty(feed.Uri))
                return null;

            return new Feed
            {
                Name = feed.Name,
                Image = null,
                Uri = feed.Uri,
                IsEnabled = existingFeeds.Any(o => o.Uri.Equals(feed.Uri, StringComparison.OrdinalIgnoreCase)),
                ViewType = feed.ArticleViewingType,
            };
        }

        Weave.ViewModels.Feed Parse(Feed feed)
        {
            if (string.IsNullOrEmpty(feed.Uri))
                return null;

            return new Weave.ViewModels.Feed
            {
                Name = feed.Name,
                Uri = feed.Uri, 
                Category = category,
                ArticleViewingType = feed.ViewType,
            };
        }

        #endregion
    }
}