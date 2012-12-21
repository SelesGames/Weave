using SelesGames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using weave.Data;
using Weave.LiveTile.ScheduledAgent;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace weave
{
    public class MainPageViewModel : INotifyPropertyChanged, IDisposable
    {
        static string lastCategory;

        public int currentPage = 0;
        int pageSize = AppSettings.Instance.NumberOfNewsItemsPerMainPage;
        int numberOfPages = 1;
        List<FeedSource> feeds;
        List<NewsItem> allNews;
        List<NewsItem> displayedNews;
        List<NewsItem> previouslyDisplayedNews = new List<NewsItem>();
        MainPage view;
        SerialDisposable subscriptionHandle = new SerialDisposable();
        SerialDisposable progressBarVisHandle = new SerialDisposable();
        CompositeDisposable disposables = new CompositeDisposable();
        Weave4DataAccessLayer dataLayer;
        TombstoneState tombstoneState;
        RefreshListenerBase refreshListener;

        internal OperatingMode currentOperatingMode;

        internal enum OperatingMode
        {
            Category,
            Feed,
            Favorites
        }

        static IScheduler scheduler = ViewModelBackgroundScheduler.Instance;


        public string Header { get; private set; }
        public bool IsProgressBarVisible { get; private set; }
        public Visibility ProgressBarVisibility { get; private set; }
        public Guid FeedId { get; set; }


        public MainPageViewModel(MainPage view, string header)
        {
            this.view = view;
            Header = header;

            IsProgressBarVisible = false;
            ProgressBarVisibility = Visibility.Collapsed;
            NewItemCount = "0 NEW";

            dataLayer = ServiceResolver.Get<Weave4DataAccessLayer>();
        }

        public async Task InitializeAsync()
        {
            await RecoverSavedTombstoneState();
            OnNavigatedTo();
        }




        #region Getting and Saving TombstoneState

        async Task RecoverSavedTombstoneState()
        {
            tombstoneState = await AppSettings.Instance.TombstoneState.Get();
            if (tombstoneState == null)
                return;

            if (Header != lastCategory)
                currentPage = 0;

            else
                currentPage = tombstoneState.ArticleListCurrentPage;

            lastCategory = Header;
        }

        internal void SaveTransientState()
        {
            if (tombstoneState != null)
                tombstoneState.ArticleListCurrentPage = currentPage;
        }

        #endregion




        public void OnNavigatedTo()
        {
            InitializeRelevantFeedsAndSubscribeToUpdates();
            UpdateNewsList();
        }

        void UpdateNewsList()
        {
            InitializeAllNews();
            UpdateNewItemCount();
            InitializeNewsForCurrentPage();
        }

        void InitializeRelevantFeedsAndSubscribeToUpdates()
        {
            var tFeeds = dataLayer.Feeds.Get().WaitOnResult();

            if (currentOperatingMode == OperatingMode.Category)
            {
                if (Header.Equals("all news", StringComparison.OrdinalIgnoreCase))
                    feeds = tFeeds.ToList();
                else
                    feeds = tFeeds.OfCategory(Header).ToList();

                refreshListener = new CategoryRefreshListener(Header);
            }
            else if (currentOperatingMode == OperatingMode.Feed)
            {
                feeds = tFeeds.Where(o => FeedId.Equals(o.Id)).ToList();
                refreshListener = new FeedRefreshListener(FeedId);
            }
            else if (currentOperatingMode == OperatingMode.Favorites)
            {
                feeds = null;
            }
        }

        void InitializeAllNews()
        {
            if (currentOperatingMode == OperatingMode.Category || currentOperatingMode == OperatingMode.Feed)
            {
                allNews = feeds.AllOrderedNews().Distinct(NewsItemComparer.Instance).ToList();
            }
            else if (currentOperatingMode == OperatingMode.Favorites)
            {
                var tFeeds = dataLayer.Feeds.Get().WaitOnResult();
                allNews = tFeeds.AllNews().Where(o => o.IsFavorite).OrderByDescending(o => o.PublishDateTime).ToList();
            }
        }

        void UpdateNewItemCount()
        {
            var newItemCount = allNews.Where(o => o.IsNew()).Count();

            GlobalDispatcher.Current.BeginInvoke(() => NewItemCount = newItemCount.ToString() + " NEW");
        }

        public string NewItemCount
        {
            get { return newItemCount; }
            set { newItemCount = value; PropertyChanged.Raise(this, "NewItemCount"); }
        }
        string newItemCount;

        void InitializeNewsForCurrentPage()
        {
            var refreshedNews = allNews.Skip(pageSize * currentPage).Take(pageSize).ToList();

            bool areItemsNew = !Enumerable.SequenceEqual(refreshedNews, previouslyDisplayedNews, NewsItemComparer.Instance);

            if (areItemsNew)
            {
                displayedNews = refreshedNews;
                GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews));
                previouslyDisplayedNews = displayedNews;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;




        #region Mark Page Read

        internal void MarkCurrentPageRead()
        {
            scheduler.SafelySchedule(() => markCurrentPageRead());
        }

        async Task markCurrentPageRead()
        {
            List<NewsItem> currentPagesNewsItems;

            if (displayedNews == null)
                return;
            currentPagesNewsItems = displayedNews.ToList();
            await currentPagesNewsItems.Select(o => GlobalDispatcher.Current.InvokeAsync(() => o.HasBeenViewed = true));
            UpdateNewItemCount();
        }

        #endregion




        internal void ManualRefresh()
        {
            scheduler.SafelySchedule(() => refresh(_ => true));
        }

        internal void AutoRefresh()
        {
            var now = DateTime.UtcNow;
            scheduler.SafelySchedule(() => refresh(o => (now - o.LastRefreshedOn) > FeedSource.RefreshThreshold));
        }

        async Task refresh(Func<FeedSource, bool> predicate)
        {
            if (feeds == null)
                return;

            FeedSource.NewsServer.BeginFeedUpdateBatch();
            foreach (var feed in feeds.Where(predicate))
                feed.RefreshNews();
            FeedSource.NewsServer.EndFeedUpdateBatch();

            var task = refreshListener.GetRefreshed();

            if (!task.IsCompleted)
            {
                ShowProgressBar();
                await refreshListener.GetRefreshed();
                HideProgressBar();
                await Task.Yield();
                UpdateNewsList();
            }
        }

        void ShowProgressBar()
        {
            IsProgressBarVisible = true;
            ProgressBarVisibility = Visibility.Visible;
            GlobalDispatcher.Current.BeginInvoke(() =>
            {
                PropertyChanged.Raise(this, "IsProgressBarVisible");
                PropertyChanged.Raise(this, "ProgressBarVisibility");
            });
        }

        void HideProgressBar()
        {
            IsProgressBarVisible = false;
            ProgressBarVisibility = Visibility.Collapsed;
            GlobalDispatcher.Current.BeginInvoke(() =>
            {
                PropertyChanged.Raise(this, "IsProgressBarVisible");
                PropertyChanged.Raise(this, "ProgressBarVisibility");
            });
        }



        #region Live Tile Creation

        public async Task<CycleTileViewModel> CreateLiveTileViewModel()
        {
            var imageUris = await allNews.CreateImageUrisFromNews(TimeSpan.FromSeconds(3));

            return new CycleTileViewModel
            {
                AppName = AppSettings.Instance.AppName.ToUpperInvariant() + " | " + Header.ToTitleCase(),
                SmallBackgroundImageUri = new Uri(@"Assets\Tiles\CycleTileSmall.png", UriKind.Relative),
                NewCount = 0,
                ImageIsoStorageUris = imageUris,
            };
        }

        #endregion




        public void Dispose()
        {
            disposables.Dispose();

            if (this.subscriptionHandle != null)
                this.subscriptionHandle.Dispose();

            if (this.progressBarVisHandle != null)
                this.progressBarVisHandle.Dispose();
        }

        ~MainPageViewModel()
        {
            DebugEx.WriteLine("MainPageViewModel {0} was finalized", this.Header);
        }
    }
}