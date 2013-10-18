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
        UserInfo user;

        public ObservableCollection<ObservableGroup<Feed, string>> FeedGroups { get; private set; }
        public string SourcesCount { get; set; }

        public ManageSourcesViewModel()
        {
            FeedGroups = new ObservableCollection<ObservableGroup<Feed, string>>();
            user = ServiceResolver.Get<IUserCache>().Get();
        }

        public async Task LoadFeeds()
        {
            if (user.AreFeedsModified)
                await user.LoadFeeds();

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
            PropertyChanged.Raise(this, "SourcesCount");
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
            await user.RemoveFeed(feed);
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
