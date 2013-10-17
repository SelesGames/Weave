﻿using System;
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

        public NewsItemGroup FindByCategory(string categoryName)
        {
            return Feeds.OfType<CategoryGroup>().FirstOrDefault(o => o.DisplayName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        }

        public NewsItemGroup FindByFeedId(Guid feedId)
        {
            return Feeds.OfType<FeedGroup>().FirstOrDefault(o => o.Feed.Id.Equals(feedId));
        }

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

        void RefreshFeeds()
        {
            Feeds.Clear();

            if (user.Feeds == null)
                return;

            foreach (var source in GetAllSources(user.Feeds))
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




        #region Helper methods

        IEnumerable<NewsItemGroup> GetAllSources (IEnumerable<Feed> feeds)
        {
            var groupedFeeds = feeds.GroupBy(o => o.Category).ToList();

            var categories = groupedFeeds
                .Where(o => !string.IsNullOrEmpty(o.Key))
                .Select(o => new CategoryGroup(user, o.Key, o))
                .OrderBy(o => o.DisplayName)
                .ToList();

            var looseFeeds = groupedFeeds
                .Where(o => string.IsNullOrEmpty(o.Key))
                .SelectMany(o => o)
                .Where(o => o.Name != null)
                .Select(o => new FeedGroup(user, o, null))
                .OrderBy(o => o.DisplayName)
                .ToList();

            var sources = new List<NewsItemGroup>();

            sources.Add(new CategoryGroup(user, "all news", categories.SelectMany(o => o.Feeds).Union(looseFeeds)));

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