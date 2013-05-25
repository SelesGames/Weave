using SelesGames;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;

namespace weave
{
    public class ManageSourcesViewModel : INotifyPropertyChanged
    {
        IUserCache userCache = ServiceResolver.Get<IUserCache>();
        UserInfo user;

        public ObservableCollection<ObservableGroup<Feed, string>> FeedGroups { get; private set; }
        public string SourcesCount { get; set; }
        public bool AreThereTooManyFeeds { get; set; }
        public string Warning { get; set; }
        public Visibility WarningVisibility { get; set; }

        public ManageSourcesViewModel()
        {
            WarningVisibility = Visibility.Collapsed;
            Warning = string.Format(
                "You have more than {0} feeds.  For performance reasons, there is a limit of {1} sources.  Please delete some now.",
                100,//Weave4DataAccessLayer.MaxAllowedSources,
                100);//Weave4DataAccessLayer.MaxAllowedSources);

            FeedGroups = new ObservableCollection<ObservableGroup<Feed, string>>();
            user = userCache.Get();
        }

        public async Task LoadFeeds()
        {
            if (user.ShouldRefreshFeedsInfo())
                await user.RefreshFeedsInfo();

            FeedGroups.Clear();

            var feeds = user.Feeds;

            var groupedFeeds = feeds
                .GroupBy(o => o.Category)
                .OrderBy(o => o.Key)
                .Select(group => new ObservableGroup<Feed, string>(Uppercase(group.Key), group.OrderBy(o => o.Name)))
                .ToList();

            var looseFeeds = groupedFeeds.SingleOrDefault(o => o.Key == null);
            if (looseFeeds != null)
            {
                groupedFeeds.Remove(looseFeeds);
                looseFeeds.Key = "CATEGORY: NONE";
                groupedFeeds.Add(looseFeeds);
            }

            foreach (var group in groupedFeeds)
                FeedGroups.Add(group);

            ReevaluateNumberOfFeeds();
        }

        void ReevaluateNumberOfFeeds()
        {
            var feedCount = FeedGroups.SelectMany(o => o).Count();
            SourcesCount = string.Format("({0})", feedCount);
            AreThereTooManyFeeds = feedCount > 100;// feeds.AreThereTooManyFeeds();
            WarningVisibility = AreThereTooManyFeeds ? Visibility.Visible : Visibility.Collapsed;
            PropertyChanged.Raise(this, "SourcesCount");
            PropertyChanged.Raise(this, "WarningVisibility");
        }

        string Uppercase(string p)
        {
            if (string.IsNullOrEmpty(p))
                return p;
            else 
                return p.ToUpperInvariant();
        }

        public async Task DeleteSource(Feed feed)
        {
            await userCache.Get().RemoveFeed(feed);
            foreach (var group in FeedGroups)
                group.Remove(feed);
            ReevaluateNumberOfFeeds();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ObservableGroup<T, TKey> : ObservableCollection<T>
    {
        public TKey Key { get; set; }

        public ObservableGroup(TKey key, IEnumerable<T> items)
        {
            this.Key = key;
            foreach (T item in items)
            {
                this.Add(item);
            }
        }
    }
}
