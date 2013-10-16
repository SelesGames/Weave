using SelesGames;
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

        public ObservableCollection<NewsItemGroup> Feeds { get; private set; }

        public FeedsListenerViewModel(UserInfo user)
        {
            this.user = user;

            if (user == null)
                throw new ArgumentNullException("user");

            user.PropertyChanged += user_PropertyChanged;
            user.Feeds.CollectionChanged += Feeds_CollectionChanged;

            Feeds = new ObservableCollection<NewsItemGroup>();

            RefreshFeeds();
        }

        public void SetNewCountToZero(NewsItemGroup catVM)
        {
            catVM.MarkEntry();
            //if (catVM.Name.Equals("all news", StringComparison.OrdinalIgnoreCase))
            //{
            //    catVM.NewArticleCount = 0;
            //    return;
            //}
            //else
            //{
            //    var allNewsCatVM = Feeds.FirstOrDefault(o => o.Name.Equals("all news", StringComparison.OrdinalIgnoreCase));
            //    if (allNewsCatVM != null)
            //    {
            //        allNewsCatVM.NewArticleCount -= catVM.NewArticleCount;
            //    }
            //    catVM.NewArticleCount = 0;
            //}
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
        public static IEnumerable<NewsItemGroup> GetAllSources(this IEnumerable<Feed> feeds, Func<string, string> categoryCasing, Func<string, string> feedCasing)
        {
            var user = ServiceResolver.Get<UserInfo>();

            var groupedFeeds = feeds.GroupBy(o => o.Category).ToList();

            var categories = groupedFeeds
                .Where(o => !string.IsNullOrEmpty(o.Key))
                .Select(o => new CategoryGroup(user, o.Key, o))
                .OrderBy(o => o.DisplayName);

            var looseFeeds = groupedFeeds
                .Where(o => string.IsNullOrEmpty(o.Key))
                .SelectMany(o => o)
                .Where(o => o.Name != null)
                .Select(o => new FeedGroup(user, o, null))
                .OrderBy(o => o.DisplayName);

            var sources = new List<NewsItemGroup>();

            sources.Add(new CategoryGroup(user, "all news", feeds));

            var all = (categories.Cast<NewsItemGroup>()).Union(looseFeeds.Cast<NewsItemGroup>());
            sources.AddRange(all);

            return sources;
        }

        public static IEnumerable<NewsItemGroup> GetAllSources(this IEnumerable<Feed> feeds)
        {
            return GetAllSources(feeds, null, null);
        }
    }
}