using SelesGames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using weave.Data;

namespace weave
{
    public class ManageSourcesViewModel : INotifyPropertyChanged
    {
        Weave4DataAccessLayer dataRepo;

        public ObservableCollection<ObservableGroup<FeedSource, string>> FeedGroups { get; private set; }
        public string SourcesCount { get; set; }
        public bool AreThereTooManyFeeds { get; set; }
        public string Warning { get; set; }
        public Visibility WarningVisibility { get; set; }

        public ManageSourcesViewModel()
        {
            WarningVisibility = Visibility.Collapsed;
            Warning = string.Format(
                "You have more than {0} feeds.  For performance reasons, there is a limit of {1} sources.  Please delete some now.",
                Weave4DataAccessLayer.MaxAllowedSources,
                Weave4DataAccessLayer.MaxAllowedSources);
            FeedGroups = new ObservableCollection<ObservableGroup<FeedSource, string>>();
            dataRepo = ServiceResolver.Get<Weave4DataAccessLayer>();
        }

        public async Task LoadFeedsAsync()
        {
            FeedGroups.Clear();

            var feeds = await dataRepo.Feeds.Get();

            var groupedFeeds = feeds
                .GroupBy(o => o.Category)
                .OrderBy(o => o.Key)
                .Select(group => new ObservableGroup<FeedSource, string>(Uppercase(group.Key), group.OrderBy(o => o.FeedName)))
                .ToList();

            var noCategories = groupedFeeds.SingleOrDefault(o => o.Key == null);
            if (noCategories != null)
            {
                groupedFeeds.Remove(noCategories);
                noCategories.Key = "CATEGORY: NONE";
                groupedFeeds.Add(noCategories);
            }

            foreach (var group in groupedFeeds)
                FeedGroups.Add(group);

            ReevaluateNumberOfFeeds();
        }

        void ReevaluateNumberOfFeeds()
        {
            var feeds = FeedGroups.SelectMany(o => o).ToList();
            SourcesCount = string.Format("({0})", feeds.Count);
            AreThereTooManyFeeds = feeds.AreThereTooManyFeeds();
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

        public void DeleteSource(FeedSource feed)
        {
            dataRepo.DeleteFeed(feed);
            foreach (var group in FeedGroups)
                group.Remove(feed);
            ReevaluateNumberOfFeeds();
        }

        public async Task SaveChanges()
        {
            await dataRepo.SaveFeeds();
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
