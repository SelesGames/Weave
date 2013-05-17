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
    public class AsyncNewsList
    {
        public Func<Task<List<NewsItem>>> News { get; set; }
    }

    public class PagedNews
    {
        NewsList currentNewsList;
        List<AsyncNewsList> newsLists = new List<AsyncNewsList>();
        BaseNewsCollectionViewModel vm;

        public int PageSize { get; private set; }
        public int NumberOfPagesToTakeAtATime { get; private set; }
        public int PageCount { get; private set; }
        public int TotalNewsCount { get; private set; }
        public int NewNewsCount { get; private set; }

        public PagedNews(BaseNewsCollectionViewModel vm, int pageSize, int numberOfPagesToTakeAtATime)
        {
            this.vm = vm;
            PageSize = pageSize;
            NumberOfPagesToTakeAtATime = numberOfPagesToTakeAtATime;
        }

        public async Task Refresh()
        {
            var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
            var awaitHandle = vm.GetNewsList(false, true, 0, takeAmount);
            currentNewsList = await awaitHandle;
            PageCount = currentNewsList.GetPageCount(PageSize);
            TotalNewsCount = currentNewsList.TotalNewsCount;
            NewNewsCount = currentNewsList.NewNewsCount;

            for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
            {
                var skipMult = j;
                var t = new AsyncNewsList
                {
                    News = () => Task.FromResult(currentNewsList.News.Skip(skipMult * PageSize).Take(PageSize).ToList()),
                };

                newsLists.Add(t);
            }

            Chunk();
        }

        void Chunk()
        {
            for (int i = NumberOfPagesToTakeAtATime; i < PageCount; i += NumberOfPagesToTakeAtATime)
            {
                var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
                var skipAmount = i * PageSize;
                var load = Lazy.Create(() => vm.GetNewsList(false, false, skipAmount, takeAmount));

                for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
                {
                    var skipMult = j;
                    var t = new AsyncNewsList
                    {
                        News = async () => (await load.Get()).News.Skip(skipMult * PageSize).Take(PageSize).ToList(),
                    };

                    newsLists.Add(t);
                }
            }
        }

        public AsyncNewsList GetNewsFuncForPage(int page)
        {
            return newsLists[page];
        }
    }


    public class MainPageViewModel : INotifyPropertyChanged, IDisposable
    {
        static string lastCategory;

        public int currentPage = 0;
        //int pageSize = AppSettings.Instance.NumberOfNewsItemsPerMainPage;
        int numberOfPages = 1;

        //List<NewsItem> allNews;
        List<NewsItem> displayedNews;
        List<NewsItem> previouslyDisplayedNews = new List<NewsItem>();
        MainPage view;
        SerialDisposable subscriptionHandle = new SerialDisposable();
        SerialDisposable progressBarVisHandle = new SerialDisposable();
        CompositeDisposable disposables = new CompositeDisposable();
        TombstoneState tombstoneState;

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
            view.IsPreviousButtonEnabled = HasPrevious = false;
            view.IsNextButtonEnabled = HasNext = false;
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
                await pageNews.Refresh();
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
            //var t = x.News();
            var refreshedNews = await x.News();// allNews.Skip(pageSize * currentPage).Take(pageSize).ToList();

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
            GlobalDispatcher.Current.BeginInvoke(() =>
            {
                view.IsPreviousButtonEnabled = HasPrevious;
                view.IsNextButtonEnabled = HasNext;
            });
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
            scheduler.SafelySchedule(() => refresh());//_ => true));
        }

        //internal void AutoRefresh()
        //{
        //    ManualRefresh();
        //    //var now = DateTime.UtcNow;
        //    //scheduler.SafelySchedule(() => refresh(o => (now - o.LastRefreshedOn) > FeedSource.RefreshThreshold));
        //}

        async Task refresh()//Func<FeedSource, bool> predicate)
        {
            ShowProgressBar();
            await pageNews.Refresh();// newsCollectionVM.RefreshNews();
            HideProgressBar();
            await Task.Yield();
            UpdateNewsList();
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