using Portable.Common.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Weave.ViewModels;

namespace weave
{
    public class FeedsToNewsItemGroupAdapter : IDisposable
    {
        UserInfo user;

        public ObservableCollectionEx<NewsItemGroup> Feeds { get; private set; }

        public FeedsToNewsItemGroupAdapter(UserInfo user)
        {
            this.user = user;

            if (user == null)
                throw new ArgumentNullException("user");

            user.PropertyChanged += user_PropertyChanged;
            user.Feeds.CollectionChanged += Feeds_CollectionChanged;

            Feeds = new ObservableCollectionEx<NewsItemGroup>();

            RefreshFeeds();
        }

        public NewsItemGroup Find(string categoryName)
        {
            return Feeds
                .Where(o => o is CategoryGroup || o is AllNewsGroup)
                .FirstOrDefault(o => o.DisplayName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        }

        public NewsItemGroup Find(Guid feedId)
        {
            return Feeds.OfType<FeedGroup>().FirstOrDefault(o => o.Feed.Id.Equals(feedId));
        }

        void RefreshFeeds()
        {

            if (user.Feeds == null)
            {
                Feeds.Clear();
                return;
            }

            Feeds.ClearAndAddRange(GetAllSources(user.Feeds));
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




        #region Helper methods

        IEnumerable<NewsItemGroup> GetAllSources (IEnumerable<Feed> feeds)
        {
            var allNews = new AllNewsGroup(user);

            var groupedFeeds = feeds.GroupBy(o => o.Category).ToList();

            var categories = groupedFeeds
                .Where(o => !string.IsNullOrEmpty(o.Key))
                .Select(o => new CategoryGroup(user, o.Key, o, allNews))
                .OrderBy(o => o.DisplayName)
                .ToList();

            var looseFeeds = groupedFeeds
                .Where(o => string.IsNullOrEmpty(o.Key))
                .SelectMany(o => o)
                .Where(o => o.Name != null)
                .Select(o => new FeedGroup(user, o, null, allNews))
                .OrderBy(o => o.DisplayName)
                .ToList();

            allNews.Subgroups = categories.SelectMany(o => o.Feeds).Union(looseFeeds).ToList();

            var sources = new List<NewsItemGroup>();
            sources.Add(allNews);

            var all = (categories.Cast<NewsItemGroup>()).Union(looseFeeds.Cast<NewsItemGroup>());
            sources.AddRange(all);
            return sources;
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
}