using System;
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
}