using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using System.Windows.Threading;


namespace weave
{
    public partial class ExpMainPage : PhoneApplicationPage, IDisposable
    {
        MainPageViewModel vm;
        bool isPopupShowing;
        bool hasBeenLoaded = false;
        ImageCache imageCache;
        ScrollViewer currentListBoxScroller;
        NewsItem engagedNewsItem;
        UIElement lastHiddenNewsItem;
        bool isInArticleSwipeMode;
        CompositeDisposable disposables = new CompositeDisposable();

        public ExpMainPage()
        {
            InitializeComponent();
            InitializeApplicationBarButtonsInListView();
       
            if (DesignerProperties.IsInDesignTool)
                return;

            detailedArticleViewer.DataContext = null;

            Debug.WriteLine("\r\n\r\nMAIN THREAD HAPPENING ON THREAD {0}\r\n\r\n", Thread.CurrentThread.ManagedThreadId);

            //Observable.Start(() => new LoadNewsItemsCacheTask(), OneThreadScheduler.Instance);
            //Observable.Start(() => NewsItemService.RefreshAllFeeds());// new LoadNewsItemsCacheTask());

            SubscribeToNewsItemClicked();

            detailedArticleViewer.HideStarted += (s, e) => OnNewsItemDismissed();
            detailedArticleViewer.HideCompleted += (s, e) => OnNewsItemHidden();

            var disp = FeedSource.WebRequestQueue.RequestStatusChanged
                .ObserveOnDispatcher()
                .Subscribe(status =>
                {
                    DebugEx.WriteLine("Status changed to: {0}", status.ToString());
                    if (status == System.Net.HttpWebRequestQueue.RequestStatus.NoPendingRequests)
                    {
                        CountdownTimer.In(TimeSpan.FromSeconds(1)).Do(() =>
                        {
                            if (vm != null)
                                vm.ForceNewsRefresh();
                        });
                        progressBar.IsIndeterminate = false;
                        progressBar.Visibility = Visibility.Collapsed;
                    }
                    else if (status == System.Net.HttpWebRequestQueue.RequestStatus.OutstandingRequests)
                    {
                        progressBar.IsIndeterminate = true;
                        progressBar.Visibility = Visibility.Visible;
                    }
                });

            disposables.Add(disp);

            currentListBoxScroller = cl.scroller;
            imageCache = new ImageCache();
            cl.SetImageCache(imageCache);
            detailedArticleViewer.ImageCache = imageCache;
            //SubscribeToFlickGestures(currentListBoxScroller);
            //previousPageStartSB.Completed += (s, e) => listBitmap.Visibility = Visibility.Collapsed;
            //nextPageStartSB.Completed += (s, e) => listBitmap.Visibility = Visibility.Collapsed;

            var gestureListener = GestureService.GetGestureListener(ContentGrid);

            disposables.Add(
            Observable.FromEvent<FlickGestureEventArgs>(
                e => gestureListener.Flick += e,
                e => gestureListener.Flick -= e)
                //.Where(notUsed => !isPopupShowing)
                .Subscribe(o => GestureListener_Flick(o.Sender, o.EventArgs)));
        }



        IDisposable disp1, disp2, disp3;

        void SubscribeToNewsItemClicked()
        {
            var disp = cl.NewsItemSelected
                .Where(notUsed => !isPopupShowing)
                .ObserveOnDispatcher()
                .Subscribe(newsItem => OnNewsItemClicked(newsItem));

            disposables.Add(disp);
        }

        void OnNewsItemClicked(Tuple<object, NewsItem> newsItem)
        {
            DisposeOfAnimationNotifications();
            isInArticleSwipeMode = false;
            isPopupShowing = true;
            engagedNewsItem = newsItem.Item2;
            vm.SetEngagedNewsItem(engagedNewsItem);
            ApplicationBar.IsVisible = false;
            lastHiddenNewsItem = newsItem.Item1 as UIElement;
            var transform = lastHiddenNewsItem.TransformToVisual(ContentGrid);
            Point point = transform.Transform(new Point(0, 0));
            double myObjTop = point.Y;
            lastHiddenNewsItem.Opacity = 0;

            disp1 = FadeNewsOutSB.BeginWithNotification()
                .Subscribe(o => cl.Visibility = Visibility.Collapsed);

            disp2 = detailedArticleViewer.Show(newsItem.Item2, myObjTop)
                .Subscribe(o =>
                {
                    InitializeApplicationBarButtonsInPopupView();
                    ApplicationBar.IsVisible = true;
                    TapTextFadeInSB.Begin();
                });
        }

        void OnNewsItemDismissed()
        {
            DisposeOfAnimationNotifications();

            isPopupShowing = false;
            engagedNewsItem = null;
            ApplicationBar.IsVisible = false;
            lastHiddenNewsItem.Opacity = 1;
            TapTextFadeOutSB.Begin();
        }

        void OnNewsItemHidden()
        {
            cl.Visibility = Visibility.Visible;

            disp3 = FadeNewsInSB.BeginWithNotification()
                .Subscribe(o =>
                {
                    InitializeAppMenuInListView(vm.Category);
                    InitializeApplicationBarButtonsInListView();
                    vm.ReevaluateNextAndPreviousButtonsVisibilities();
                    ApplicationBar.IsVisible = true;
                });
        }

        void DisposeOfAnimationNotifications()
        {
            if (disp1 != null)
                disp1.Dispose();
            if (disp2 != null)
                disp2.Dispose();
            if (disp3 != null)
                disp3.Dispose();
        }



        #region Mark All Read

        void OnAllRead()
        {
            vm.MarkCurrentPageRead();
        }

        #endregion



        #region Touch Swiping handling

        void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction != System.Windows.Controls.Orientation.Horizontal)
                return;

            var velocity = e.HorizontalVelocity;

            Debug.WriteLine("h vel: {0}", velocity);

            if (!isPopupShowing)
            {
                if (velocity < -750)
                    OnNextPage();

                else if (velocity > 700)
                    OnPreviousPage();
            }
            else
            {
                if (velocity < -750)
                {
                    var nextNewsItem = vm.GetNextNewsItem();
                    if (nextNewsItem != null)
                    {
                        isInArticleSwipeMode = true;
                        engagedNewsItem = nextNewsItem;
                        detailedArticleViewer.NextArticle(engagedNewsItem);
                    }
                }

                else if (velocity > 700)
                {
                    var previousNewsItem = vm.GetPreviousNewsItem();
                    if (previousNewsItem != null)
                    {
                        isInArticleSwipeMode = true;
                        engagedNewsItem = previousNewsItem;
                        detailedArticleViewer.PreviousArticle(engagedNewsItem);
                    }
                }
            }
        }

        //void SubscribeToFlickGestures(UIElement uiElement)
        //{
        //    var manipulationStarted = new Subject<ManipulationStartedEventArgs>();
        //    var manipulationDelta = new Subject<ManipulationDeltaEventArgs>();

        //    uiElement.AddHandler(
        //        UIElement.ManipulationStartedEvent,
        //        new EventHandler<ManipulationStartedEventArgs>((s, e) => manipulationStarted.OnNext(e)),
        //        true);
        //    uiElement.AddHandler(
        //        UIElement.ManipulationDeltaEvent,
        //        new EventHandler<ManipulationDeltaEventArgs>((s, e) => manipulationDelta.OnNext(e)),
        //        true);

        //    double threshold = 240;

        //    var flick = from started in manipulationStarted
        //                let startTime = DateTime.Now
        //                from delta in manipulationDelta
        //                let elapsedSeconds = (startTime - DateTime.Now).TotalSeconds
        //                let totalTranslation = delta.CumulativeManipulation.Translation
        //                let velocity = new Point(
        //                    totalTranslation.X / elapsedSeconds,
        //                    totalTranslation.Y / elapsedSeconds)
        //                select velocity;

        //    var leftAndRightFlicks = flick
        //        .Where(velocity => Math.Abs(velocity.X) > Math.Abs(velocity.Y));

        //    var validRightFlicks = leftAndRightFlicks.Where(velocity => velocity.X < -threshold).Take(1).Repeat();
        //    var validLeftFlicks = leftAndRightFlicks.Where(velocity => velocity.X > threshold).Take(1).Repeat();

        //    leftFlickHandle = validLeftFlicks.Subscribe(i => OnNextPage());
        //    rightFlickHandle = validRightFlicks.Subscribe(i => OnPreviousPage());
        //}

        #endregion



        #region Page Change logic

        public bool IsPreviousButtonEnabled
        {
            get { return previousButton.IsEnabled; }
            set { previousButton.IsEnabled = value; }
        }

        public bool IsNextButtonEnabled
        {
            get { return nextButton.IsEnabled; }
            set { nextButton.IsEnabled = value; }
        }

        void OnPreviousPage()
        {
            if (vm.HasPrevious)
            {
                PreparePageChangeAnimation(PageChangeAnimateDirection.Previous);
                Observable.Start(() => imageCache.Flush());
                vm.CurrentPage--;
            }
        }

        void OnNextPage()
        {
            if (vm.HasNext)
            {
                PreparePageChangeAnimation(PageChangeAnimateDirection.Next);
                Observable.Start(() => imageCache.Flush());
                vm.CurrentPage++;
            }
        }

        enum PageChangeAnimateDirection
        {
            Previous,
            Next
        }

        #endregion



        #region Animation Helpers

        bool isInPageChangeAnimation = false;

        void PreparePageChangeAnimation(PageChangeAnimateDirection direction)
        {
            if (isInPageChangeAnimation)
                return;

            isInPageChangeAnimation = true;
            SetListBitmapSource();

            cl.Visibility = Visibility.Collapsed;

            currentListBoxScroller.ScrollToVerticalOffset(0);

            IDisposable disp = null;
            if (direction == PageChangeAnimateDirection.Previous)
                disp = previousPageStartSB.BeginWithNotification().Subscribe(o => {listBitmap.Visibility = Visibility.Collapsed; disp.Dispose();});
            else if (direction == PageChangeAnimateDirection.Next)
                disp = nextPageStartSB.BeginWithNotification().Subscribe(o => { listBitmap.Visibility = Visibility.Collapsed; disp.Dispose(); });

            markAllReadButton.IsEnabled = false;
        }

        void SetListBitmapSource()
        {
            try
            {
                WriteableBitmap bitmap = new WriteableBitmap(cl, null);
                listBitmap.Source = bitmap;
            }
            catch (Exception) { }
            listBitmap.Visibility = Visibility.Visible;
        }

        internal void CompletePageChangeAnimation(List<NewsItem> source, int direction = 0)
        {
            isInPageChangeAnimation = false;
            cl.SetNews(source, direction);
            cl.Visibility = Visibility.Visible;
            markAllReadButton.IsEnabled = true;
        }

        #endregion



        #region ApplicationBar Construction


        #region Initialize the ApplicationBar when looking at List View

        void InitializeApplicationBarButtonsInListView()
        {
            ApplicationBar.Buttons.Clear();
            markAllReadButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.allread.png", UriKind.Relative), Text = "all read" };
            previousButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.back.rest.png", UriKind.Relative), Text = "previous" };
            nextButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.next.rest.png", UriKind.Relative), Text = "next" };
            markAllReadButton.Click += (s, e) => OnAllRead();
            previousButton.Click += (s, e) => OnPreviousPage();
            nextButton.Click += (s, e) => OnNextPage();
            ApplicationBar.Buttons.Add(markAllReadButton);
            ApplicationBar.Buttons.Add(previousButton);
            ApplicationBar.Buttons.Add(nextButton);
        }

        void InitializeAppMenuInListView(Category excludedCategory)
        {
            ApplicationBar.MenuItems.Clear();
            var refreshButton = new ApplicationBarMenuItem { Text = "refresh" };
            refreshButton.Click += (s, e) => vm.Refresh();
            ApplicationBar.MenuItems.Add(refreshButton);                

            if (excludedCategory != null)
                ApplicationBar.MenuItems.Add(CreateMenuItem("all news"));
            foreach (var category in FeedsSettingsService.EnabledCategories.Except(new[] { excludedCategory }).OrderBy(o => o.Name).ToList())
            {
                ApplicationBar.MenuItems.Add(CreateMenuItem(category.Name));
            }
            var settingsButton = new ApplicationBarMenuItem { Text = "settings" };
            settingsButton.Click += (s,e) => GlobalNavigationService.ToSettingsPage();
            ApplicationBar.MenuItems.Add(settingsButton);                
        }

        static ApplicationBarMenuItem CreateMenuItem(string category)
        {
            var menuItem = new ApplicationBarMenuItem { Text = category };
            menuItem.Click += (s, e) => GlobalNavigationService.ToMainPage(category);
            return menuItem;
        }

        #endregion



        #region Initialize the ApplicationBar when looking at Popup View

        void InitializeApplicationBarButtonsInPopupView()
        {
            var factory = new ApplicationBarButtonFactory(
                ApplicationBarButtonFactory.AppBarMode.MainPagePopupMode, 
                this.ApplicationBar, 
                () => engagedNewsItem);

            //factory.StarPressed += (s, e) =>
            //{
            //    detailedArticleViewer.Hide();
            //    isPopupShowing = false;
            //};
            factory.MarkReadPressed += (s, e) =>
            {
                engagedNewsItem.HasBeenViewed = true;
                if (!isInArticleSwipeMode)
                    detailedArticleViewer.Hide();
            };
            factory.InAppBrowserButtonPressed += (s, e) =>
            {
                if (!isInArticleSwipeMode)
                    detailedArticleViewer.Hide();
            };
        }

        #endregion


        #endregion



        #region Navigation To/From, BackKeyPress

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (hasBeenLoaded)
            {
                if (isPopupShowing)
                    InitializeApplicationBarButtonsInPopupView();
                else
                    InitializeAppMenuInListView(vm.Category);

                vm.RefreshFeedsNewsAndCurrentPage();
                return;
            }

            hasBeenLoaded = true;

            string context = null;
            if (NavigationContext.QueryString.ContainsKey("category"))
            {
                context = NavigationContext.QueryString["category"];
            }

            IDisposable handle = null;
            handle = Observable.Start(() =>
                {
                    var category = FeedsSettingsService.GetAllCategories().SingleOrDefault(o =>
                        o.Name.ToLower() == context.ToLower());

                    string header = null;
                    if (category != null)
                        header = category.Name.ToLower();
                    else
                        header = "All News".ToLower();

                    return new { Category = category, Header = header };
                })
                .ObserveOnDispatcher()
                .Subscribe(o =>
                {
                    handle.Dispose();
                    vm = new MainPageViewModel(this, o.Category, o.Header);
                    DataContext = vm;
                    InitializeAppMenuInListView(o.Category);
                    //this.currentListBoxScroller.ScrollToVerticalOffset(vm.ScrollPosition);
                });
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            Observable.Start(() =>
            {
                //vm.ScrollPosition = this.currentListBoxScroller.VerticalOffset;
                vm.SaveTransientState();
            });
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (adControl.IsAdCurrentlyEngaged)
            {
                return;
            }
            else if (isPopupShowing)
            {
                detailedArticleViewer.Hide();
                e.Cancel = true;
            }
            else
            {
                vm.Dispose();
                listBitmap.Source = null;
                Dispose();

                Observable.Start(() =>
                {
                    imageCache.Flush();
                });
            }
        }

        #endregion



        public void Dispose()
        {
            DisposeOfAnimationNotifications();
            disposables.Dispose();
        }
    }
}