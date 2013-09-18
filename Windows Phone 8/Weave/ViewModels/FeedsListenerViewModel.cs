using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Weave.ViewModels;

namespace weave
{
    public class FeedsListenerViewModel : IDisposable
    {
        UserInfo user;

        public ObservableCollection<CategoryOrLooseFeedViewModel> Feeds { get; private set; }

        public FeedsListenerViewModel(UserInfo user)
        {
            this.user = user;

            if (user == null)
                throw new ArgumentNullException("user");

            user.PropertyChanged += user_PropertyChanged;
            user.Feeds.CollectionChanged += Feeds_CollectionChanged;

            Feeds = new ObservableCollection<CategoryOrLooseFeedViewModel>();

            RefreshFeeds();
        }

        void RefreshFeeds()
        {
            Feeds.Clear();

            if (user.Feeds == null)
                return;

            var feeds = user.Feeds;
            var sources = feeds.GetAllSources(o => o.ToUpper(), o => o).ToList();
            foreach (var source in sources)
                Feeds.Add(source);
        }




        #region Event Handlers

        void Feeds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshFeeds();
        }

        void user_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Feeds")
            {
                RefreshFeeds();
            }
        }

        #endregion




        #region IDisposable

        public void Dispose()
        {
            user.PropertyChanged -= user_PropertyChanged;
            user.Feeds.CollectionChanged -= Feeds_CollectionChanged;
        }

        #endregion
    }

    internal static class FeedSourceExtensions
    {
        public static IEnumerable<CategoryOrLooseFeedViewModel> GetAllSources(this IEnumerable<Feed> feeds, Func<string, string> categoryCasing, Func<string, string> feedCasing)
        {
            var groupedFeeds = feeds.GroupBy(o => o.Category).ToList();

            var categories = groupedFeeds
                .Where(o => !string.IsNullOrEmpty(o.Key))
                .Select(o =>
                    new CategoryOrLooseFeedViewModel
                    {
                        Name = categoryCasing(o.Key),
                        Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category,
                        NewArticleCount = o.Sum(x => x.NewArticleCount)
                    })
                .OrderBy(o => o.Name);

            var looseFeeds = groupedFeeds
                .Where(o => string.IsNullOrEmpty(o.Key))
                .SelectMany(o => o)
                .Where(o => o.Name != null)
                .Select(o =>
                    new CategoryOrLooseFeedViewModel
                    {
                        Name = feedCasing(o.Name),
                        Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed,
                        FeedId = o.Id,
                        NewArticleCount = o.NewArticleCount
                    })
                .OrderBy(o => o.Name);

            var sources = new List<CategoryOrLooseFeedViewModel>();

            sources.Add(
                new CategoryOrLooseFeedViewModel
                {
                    Name = categoryCasing("all news"),
                    Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category,
                    NewArticleCount = feeds.Sum(o => o.NewArticleCount)
                });

            sources.AddRange(categories.Union(looseFeeds));

            return sources;
        }

        public static IEnumerable<CategoryOrLooseFeedViewModel> GetAllSources(this IEnumerable<Feed> feeds)
        {
            var categories = feeds.UniqueCategoryNames()
                .Select(o => o.ToLower())
                .OrderBy(o => o)
                .Select(o => new CategoryOrLooseFeedViewModel { Name = o, Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category });

            var looseFeeds = feeds
                .Where(o => string.IsNullOrEmpty(o.Category) && o.Name != null)
                .Select(o => new { o, name = o.Name.ToLower() })
                .OrderBy(o => o.name)
                .Select(o => new CategoryOrLooseFeedViewModel { Name = o.name, Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed, FeedId = o.o.Id });


            var sources = new List<CategoryOrLooseFeedViewModel>();
            sources.Add(new CategoryOrLooseFeedViewModel { Name = "all news", Type = CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category });
            sources.AddRange(categories.Union(looseFeeds));

            return sources;
        }
    }
}