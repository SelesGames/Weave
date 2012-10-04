using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Microsoft.Phone.Shell;
using System.Data.Linq;

namespace weave
{
    public class MainPageViewModel2 : INotifyPropertyChanged, IDisposable
    {
        public int currentPage = 0;
        int pageSize = AppSettings.NumberOfNewsItemsPerMainPage;
        int numberOfPages = 1;
        List<weave.FeedLoader> feeds;
        List<NewsItem> allNews;
        int allNewsCount = 0;
        List<NewsItem> displayedNews;
        List<NewsItem> previouslyDisplayedNews = new List<NewsItem>();
        MainPage view;
        //object syncHandle = new object();
        SerialDisposable subscriptionHandle = new SerialDisposable();
        //IDisposable subscriptionHandle;
        CompositeDisposable disposables = new CompositeDisposable();


        static IScheduler scheduler = ViewModelBackgroundScheduler.Instance;


        public Category Category { get; private set; }
        public string Header { get; private set; }

        public MainPageViewModel2(MainPage view, Category category, string header)
        {
            this.view = view;
            Category = category;
            Header = header;

            //News = new ObservableCollection<NewsItemGrouping>();

            scheduler.Schedule(() =>
            {
                RecoverSavedTombstoneState();
                SaveCategoryToTombstoneState();
                onNavigatedTo();
            });
        }

        void onNavigatedTo()
        {
            InitializeRelevantFeedsAndSubscribeToUpdates();
            updateNewsList();
        }
        public void OnNavigatedTo()
        {
            scheduler.Schedule(onNavigatedTo);
            #region deprecated
            //() =>
            //{
            //    //SaveCategoryToTombstoneState();
            //    InitializeRelevantFeedsAndSubscribeToUpdates();
            //    InitializeAllNews();
            //    InitializePageCountDisplay();
            //    ReevaluateNextAndPreviousButtonsVisibilities();
            //    InitializeNewsForCurrentPage();
            //});
            #endregion
        }

        void updateNewsList()
        {
            InitializeAllNews();
            InitializePageCountDisplay();
            ReevaluateNextAndPreviousButtonsVisibilities();
            InitializeNewsForCurrentPage();
        }
        public void UpdateNewsList()
        {
            scheduler.Schedule(updateNewsList);
        }

        void RecoverSavedTombstoneState()
        {
            if (AppSettings.TombstoneState != null && AppSettings.TombstoneState.MainPageCurrentPageShouldBeFlushed)
            {
                AppSettings.TombstoneState.MainPageCurrentPageShouldBeFlushed = false;
                currentPage = 0;
            }
            else
            {
                var recoveredState = PhoneApplicationService.Current.State.GetValueOrDefaultForKey(Header + "ths2l")
                    as MainPageViewModelSavedState;

                if (recoveredState != null)
                {
                    currentPage = recoveredState.CurrentPage;
                    //ScrollPosition = recoveredState.ScrollPosition;
                }
            }
        }

        void InitializeRelevantFeedsAndSubscribeToUpdates()
        {
            var dal = ServiceResolver.Get<Data.DataAccessLayer>();

            //if (Category != null)
            //    feeds = dal.GetEnabledFeedsByCategory(Category.Name).ToList();
            //else
                feeds = dal.GetAllFeeds2().ToList();

            AttachSubscriptionHandle();
        }

        void AttachSubscriptionHandle()
        {
            subscriptionHandle.Disposable = feeds
                .Select(feed => feed.NewsWasRefreshedNotificationStream)
                .Merge()
                .Buffer(TimeSpan.FromSeconds(15))
                .Where(buffer => buffer.Count > 0)
                .ObserveOn(scheduler)
                .Subscribe(updateNewsList);
        }

        void InitializeAllNews()
        {
            ////DebugEx.WriteLine("MainPageViewModel InitializeAllNews()");
            ////lock (syncHandle)
            ////{
            //    if (Category != null)
            //        allNews = feeds
            //            .Where(o => o.News != null)
            //            .SelectMany(o => o.News)
            //            .OrderByDescending(o => o.PublishDateTime)
            //            .Distinct(NewsItemComparer.Instance)
            //            .ToList();

            //    else
            //        allNews = feeds
            //            .Where(o => o.News != null)
            //            .SelectMany(o => o.News)
            //            .OrderByDescending(o => o.PublishDateTime)
            //            .Distinct(NewsItemComparer.Instance)
            //            .ToList();
            ////}

            var context = ServiceResolver.Get<weave.Data.Sql.WeaveDataContext>();
            var newItemCount = context.NewsItems.Count();

            //var newItemCount =  allNews.Where(o => o.IsNew).Count();

            GlobalDispatcher.Current.BeginInvoke(() => NewItemCount = newItemCount.ToString() + " new");
        }

        void InitializePageCountDisplay()
        {
            var context = ServiceResolver.Get<weave.Data.Sql.WeaveDataContext>();
            allNewsCount = context.NewsItems.Count();

            numberOfPages = (int)Math.Ceiling((double)allNewsCount / (double)pageSize);
            GlobalDispatcher.Current.BeginInvoke(() => PropertyChanged.Raise(this, "CurrentPageDisplay"));
        }

        public string CurrentPageDisplay
        {
            get { return string.Format("page {0} of {1}", currentPage + 1, numberOfPages); }
        }

        public string NewItemCount
        {
            get { return newItemCount; }
            set { newItemCount = value; PropertyChanged.Raise(this, "NewItemCount"); }
        }
        string newItemCount;

        void InitializeNewsForCurrentPage()
        {
            //lock (syncHandle)
            //{

            var refreshedNews = GetPagedNews();

                bool areItemsNew = refreshedNews.Union(previouslyDisplayedNews, NewsItemComparer.Instance).Count() -
                    refreshedNews.Intersect(previouslyDisplayedNews, NewsItemComparer.Instance).Count() > 0;

                if (areItemsNew)
                {
                    displayedNews = refreshedNews;
                    GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews));
                    //GlobalDispatcher.Current.BeginInvoke(() => this.SetNewsGrouped(displayedNews));
                    previouslyDisplayedNews = displayedNews;
                }
            //}
        }

        List<NewsItem> GetPagedNews()
        {
            Func<weave.Data.Sql.WeaveDataContext, int, int, IQueryable<NewsItem>> fuckThis =
                CompiledQuery.Compile((weave.Data.Sql.WeaveDataContext c, int skip, int take) => c.Feeds
                .SelectMany(feed => feed.NewsItems)
                .OrderByDescending(newsItem => newsItem.PublishDateTime)
                    //.Distinct(o => o.Title)
                .Skip(pageSize * currentPage)
                .Take(pageSize)
                .Select(o =>
                    new NewsItem
                    {
                        Title = o.Title,
                        //Description = o.Description,
                        FeedSource = new FeedSource { FeedName = o.FeedName, },
                        HasBeenViewed = o.HasBeenViewed,
                        ImageUrl = o.ImageUrl,
                        IsNew = o.IsNew,
                        //Link = o.Link,
                        PublishDateTime = o.PublishDateTime,
                    }));

            var context = ServiceResolver.Get<weave.Data.Sql.WeaveDataContext>();

            return fuckThis(context, pageSize * currentPage, pageSize).ToList();


            //var pagedNews = context.Feeds
            //    .SelectMany(feed => feed.NewsItems)
            //    .OrderByDescending(newsItem => newsItem.PublishDateTime)
            //    //.Distinct(o => o.Title)
            //    .Skip(pageSize * currentPage)
            //    .Take(pageSize)
            //    .Select(o =>
            //        new NewsItem
            //        {
            //            Title = o.Title,
            //            //Description = o.Description,
            //            FeedSource = new FeedSource { FeedName = o.FeedName, },
            //            HasBeenViewed = o.HasBeenViewed,
            //            ImageUrl = o.ImageUrl,
            //            IsNew = o.IsNew,
            //            //Link = o.Link,
            //            PublishDateTime = o.PublishDateTime,
            //        })
            //    .ToList();

            //return pagedNews;
        }

        void SaveCategoryToTombstoneState()
        {
            if (Category != null)
                AppSettings.TombstoneState.CurrentCategory = Category.Name;
            else
                AppSettings.TombstoneState.CurrentCategory = null;
        }

        internal void ReevaluateNextAndPreviousButtonsVisibilities()
        {
            HasPrevious = (currentPage - 1) >= 0;
            HasNext = currentPage + 1 < Math.Ceiling((double)allNewsCount / (double)pageSize);
            GlobalDispatcher.Current.BeginInvoke(() =>
            {
                view.IsPreviousButtonEnabled = HasPrevious;
                view.IsNextButtonEnabled = HasNext;
            });
        }

        internal bool HasPrevious { get; private set; }
        internal bool HasNext { get; private set; }


        int lastPageLastTimeItWasSet = -1;
        //object pageChangeSync = new object();
        //IDisposable pageChangeHandle;
        SerialDisposable pageChangeHandle = new SerialDisposable();

        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                //lock (pageChangeSync)
                //{
                //    if (pageChangeHandle != null)
                //        pageChangeHandle.Dispose();
                //}
                if (value >= 0 && value < numberOfPages)
                {
                    currentPage = value;
                    ReevaluateNextAndPreviousButtonsVisibilities();
                    PropertyChanged.Raise(this, "CurrentPageDisplay");
                    //int capturedCurrentPage = currentPage;
                    pageChangeHandle.Disposable = Observable.Timer(TimeSpan.FromMilliseconds(200), scheduler).Take(1).Subscribe(notUsed =>
                    {
                        //if (currentPage == capturedCurrentPage)
                        //{
                            //lock (syncHandle)
                            //{
                        displayedNews = GetPagedNews();
                                previouslyDisplayedNews = displayedNews;
                            //}

                            if (currentPage >= lastPageLastTimeItWasSet) // we moved forward
                                GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews, 1));
                                //GlobalDispatcher.Current.BeginInvoke(() => this.SetNewsGrouped(displayedNews));

                            else
                                GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews, -1));
                                //GlobalDispatcher.Current.BeginInvoke(() => this.SetNewsGrouped(displayedNews));


                            lastPageLastTimeItWasSet = currentPage;
                        //}
                    });
                }
            }
        }
    
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SaveTransientState()
        {
            var state = new MainPageViewModelSavedState
            {
                CurrentPage = currentPage,
                //ScrollPosition = ScrollPosition,
            };
            PhoneApplicationService.Current.State.UpdateValueForKey(state, Header + "ths2l");
        }

        public class MainPageViewModelSavedState
        {
            public int CurrentPage { get; set; }
            //public double ScrollPosition { get; set; }
        }

        internal void MarkCurrentPageRead()
        {
            scheduler.Schedule(markCurrentPageRead);
        }

        void markCurrentPageRead()
        {
            if (AppSettings.TombstoneState.IsMarkAllReadEnabled)
            {
                List<NewsItem> currentPagesNewsItems;
                //lock (syncHandle)
                //{
                if (displayedNews == null)
                    return;
                currentPagesNewsItems = displayedNews.ToList();
                //}
                currentPagesNewsItems.ForEach(o => GlobalDispatcher.Current.BeginInvoke(() => o.HasBeenViewed = true));
            }
            else
            {
                GlobalDispatcher.Current.BeginInvoke(() =>
                {
                    var result = MessageBox.Show("\r\nPress \"OK\" if you want to enable Mark All Read.  \r\n\r\nOnce enabled, you won't see this prompt again during this session.\r\n\r\n", "Confirm Mark All Read", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        AppSettings.TombstoneState.IsMarkAllReadEnabled = true;
                        MarkCurrentPageRead();
                    }
                    else
                        return;
                });
            }
        }

        int currentEngagedNewsItemIndex;
        internal void SetEngagedNewsItem(NewsItem engagedNewsItem)
        {
            var pagesIndex = displayedNews.IndexOf(engagedNewsItem);
            currentEngagedNewsItemIndex = currentPage * pageSize + pagesIndex;
        }

        internal NewsItem GetNextNewsItem()
        {
            if (allNews == null || allNews.Count == 0)
                return null;

            if (currentEngagedNewsItemIndex + 1 < allNews.Count)
                return allNews[++currentEngagedNewsItemIndex];
            else
                return null;
        }

        internal NewsItem GetPreviousNewsItem()
        {
            if (allNews == null || allNews.Count == 0)
                return null;

            if (currentEngagedNewsItemIndex - 1 >= 0)
                return allNews[--currentEngagedNewsItemIndex];
            else
                return null;
        }

        internal void Refresh()
        {
            //lock (syncHandle)
            //{
            //    if (feeds == null)
            //        return;

            //    feeds.ForEach(feed => feed.CheckForAndGetNewFeedData());
            //    //feeds.ForEach(feed => feed.WeaveRefreshFeed());
            //}
            scheduler.Schedule(refresh);
        }

        void refresh()
        {
            //lock (syncHandle)
            //{
                if (feeds == null)
                    return;

                var feedsCopy = feeds.ToList();

                feedsCopy.ForEach(feed => feed.CheckForAndGetNewFeedData());
                //feeds.ForEach(feed => feed.WeaveRefreshFeed());
            //}
        }

        public void Dispose()
        {
            disposables.Dispose();

            if (this.pageChangeHandle != null)
                this.pageChangeHandle.Dispose();

            if (this.subscriptionHandle != null)
                this.subscriptionHandle.Dispose();
        }

        ~MainPageViewModel2()
        {
            DebugEx.WriteLine("MainPageViewModel {0} was finalized", this.Header);
        }
    }
}
