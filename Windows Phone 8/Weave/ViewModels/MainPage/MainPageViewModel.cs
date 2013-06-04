using SelesGames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Weave.LiveTile.ScheduledAgent;
using Weave.LiveTile.ScheduledAgent.ViewModels;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;

namespace weave
{
    public class MainPageViewModel : INotifyPropertyChanged, IDisposable
    {
        static string lastCategory;

        public int currentPage = 0;
        //int pageSize = AppSettings.Instance.NumberOfNewsItemsPerMainPage;
        int numberOfPages = 1;

        List<NewsItem> displayedNews;
        List<NewsItem> previouslyDisplayedNews = new List<NewsItem>();
        MainPage view;
        SerialDisposable subscriptionHandle = new SerialDisposable();
        SerialDisposable progressBarVisHandle = new SerialDisposable();
        CompositeDisposable disposables = new CompositeDisposable();
        TombstoneState tombstoneState;
        IUserCache userCache = ServiceResolver.Get<IUserCache>();

        PagedNews pageNews;

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
            //view.IsPreviousButtonEnabled = HasPrevious = false;
            //view.IsNextButtonEnabled = HasNext = false;
            NewItemCount = "0 NEW";
        }

        public async Task InitializeAsync()
        {
            await RecoverSavedTombstoneState();
            await OnNavigatedTo();
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




        public async Task OnNavigatedTo(NavigationMode navMode = NavigationMode.New)
        {
            if (navMode == NavigationMode.Forward || navMode == NavigationMode.New)
            {
                InitializeNewsCollectionVM();
                await pageNews.Refresh(refresh: true, markEntry: true);
                await UpdateNewsList();
            }
        }

        async Task UpdateNewsList()
        {
            UpdateNewItemCount();
            InitializePageCountDisplay();
            ReevaluateNextAndPreviousButtonsVisibilities();
            await InitializeNewsForCurrentPage();
        }

        void InitializeNewsCollectionVM()
        {
            BaseNewsCollectionViewModel newsCollectionVM = null;

            if (currentOperatingMode == OperatingMode.Category)
            {
                newsCollectionVM = new NewsCollectionCategoryViewModel(Header);
            }
            else if (currentOperatingMode == OperatingMode.Feed)
            {
                newsCollectionVM = new NewsCollectionFeedViewModel(FeedId);
            }
            else if (currentOperatingMode == OperatingMode.Favorites)
            {
                throw new Exception("shit");
                //feeds = null;
            }

            pageNews = new PagedNews(newsCollectionVM, AppSettings.Instance.NumberOfNewsItemsPerMainPage, 3);
        }

        void UpdateNewItemCount()
        {
            var newItemCount = pageNews.NewNewsCount;
            GlobalDispatcher.Current.BeginInvoke(() => NewItemCount = newItemCount.ToString() + " NEW");
        }

        void InitializePageCountDisplay()
        {
            numberOfPages = pageNews.PageCount;
            if (currentPage >= numberOfPages || Header != lastCategory)
                currentPage = 0;

            lastCategory = Header;
            GlobalDispatcher.Current.BeginInvoke(() => PropertyChanged.Raise(this, "CurrentPageDisplay"));
        }

        public string CurrentPageDisplay
        {
            get { return string.Format("PAGE {0} OF {1}", currentPage + 1, numberOfPages); }
        }

        public string NewItemCount
        {
            get { return newItemCount; }
            set { newItemCount = value; PropertyChanged.Raise(this, "NewItemCount"); }
        }
        string newItemCount;

        async Task InitializeNewsForCurrentPage()
        {
            var x = pageNews.GetNewsFuncForPage(currentPage);

            var refreshedNews = await x.News();

            bool areItemsNew = !Enumerable.SequenceEqual(refreshedNews, previouslyDisplayedNews, NewsItemComparer.Instance);

            if (areItemsNew)
            {
                displayedNews = refreshedNews;
                GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews));
                previouslyDisplayedNews = displayedNews;
            }
        }




        #region Page Change Logic

        void ReevaluateNextAndPreviousButtonsVisibilities()
        {
            HasPrevious = (currentPage - 1) >= 0;
            HasNext = currentPage + 1 < Math.Ceiling((double)pageNews.TotalNewsCount / (double)pageNews.PageSize);
        }


        internal bool HasPrevious { get; private set; }
        internal bool HasNext { get; private set; }


        int lastPageLastTimeItWasSet = -1;
        SerialDisposable pageChangeHandle = new SerialDisposable();

        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (value >= 0 && value < numberOfPages)
                {
                    currentPage = value;
                    ReevaluateNextAndPreviousButtonsVisibilities();
                    PropertyChanged.Raise(this, "CurrentPageDisplay");
                    pageChangeHandle.Disposable = Observable.Timer(TimeSpan.FromMilliseconds(200), scheduler).Take(1).Subscribe(async notUsed =>
                    {
                        bool showProgressBars = false;
                        var x = pageNews.GetNewsFuncForPage(currentPage);
                        var z = x.News();

                        if (!z.IsCompleted)
                            showProgressBars = true;

                        List<NewsItem> news = new List<NewsItem>();
                        try
                        {
                            news = await z;
                        }
                        catch (Exception ex)
                        {
                            DebugEx.WriteLine(ex);
                        }
                        displayedNews = news;
                        previouslyDisplayedNews = displayedNews;

                        if (currentPage >= lastPageLastTimeItWasSet) // we moved forward
                            GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews, 1));

                        else
                            GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews, -1));

                        lastPageLastTimeItWasSet = currentPage;
                    });
                }
            }
        }

        #endregion




        public event PropertyChangedEventHandler PropertyChanged;




        #region Mark Page Read

        internal void MarkCurrentPageRead()
        {
            scheduler.SafelySchedule(() => markCurrentPageRead());
        }

        async Task markCurrentPageRead()
        {
            if (displayedNews == null)
                return;

            await userCache.Get().MarkArticlesSoftRead(displayedNews);
            UpdateNewItemCount();
        }

        #endregion


        internal void ManualRefresh()
        {
            scheduler.SafelySchedule(() => refresh());//_ => true));
        }

        //internal void AutoRefresh()
        //{
        //    ManualRefresh();
        //    //var now = DateTime.UtcNow;
        //    //scheduler.SafelySchedule(() => refresh(o => (now - o.LastRefreshedOn) > FeedSource.RefreshThreshold));
        //}

        async Task refresh()
        {
            ShowProgressBar();
            await pageNews.Refresh(refresh: true, markEntry: false);
            HideProgressBar();
            await Task.Yield();
            await UpdateNewsList();
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
            var newsWithImages = displayedNews.Where(o => o.HasImage).ToList();
            if (newsWithImages.Count < 2)
                throw new InvalidOperationException("image count");

            var temp = Guid.NewGuid().ToString() + "photo";
            var imageUris = await newsWithImages.CreateImageUrisFromNews(temp, TimeSpan.FromSeconds(3));

            return new CycleTileViewModel
            {
                AppName = AppSettings.Instance.AppName.ToUpperInvariant() + " " + Header.ToTitleCase(),
                SmallBackgroundImageUri = new Uri(@"Assets\Tiles\CycleTileSmall.png", UriKind.Relative),
                NewCount = 0,
                ImageIsoStorageUris = imageUris,
            };
        }

        #endregion




        public void Dispose()
        {
            disposables.Dispose();

            if (this.pageChangeHandle != null)
                this.pageChangeHandle.Dispose();

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