using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using SelesGames;
using SelesGames.Phone;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace weave
{
    public partial class SamplePanorama : WeavePage
    {
        PanoramaViewModel vm;
        LatestNewsViewModel vm2;
        RefreshListenerBase currentRefreshListener;

        public SamplePanorama()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            vm = new PanoramaViewModel();
            vm2 = new LatestNewsViewModel();
            this.DataContext = vm;
            this.cat1.DataContext = vm2;

            if (AppSettings.Instance.StartupMode == StartupMode.Launch)
            {
                var permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();

                if (permState.IsFirstTime)
                {
                    permState.IsFirstTime = false;
                    AppSettings.Instance.PermanentState.Save();
                }
            }

            //dnp.Counter.EnableMemoryCounter = true;

            Debug.WriteLine("\r\n*******************\r\nMAIN GUI THREAD HAPPENING ON {0}\r\n*******************\r\n", Thread.CurrentThread.ManagedThreadId);
            this.IsHitTestVisible = false;

            vm.LoadMostViewedAsync().Wait(); ;
            vm.LoadSourcesAsync().Wait();
            SetValue(RadTransitionControl.TransitionProperty, new RadTileTransition { PlayMode = TransitionPlayMode.Manual });
        }

        protected async override void OnPageLoad(WeaveNavigationEventArgs navigationEventArgs)
        {
            if (AppSettings.Instance.LogExceptions)
                LittleWatson.LogPreviousExceptionIfPresent(loggedError =>
                    SelesGames.Phone.TaskService.ToEmailComposeTask(
                        To: "info@selesgames.com",
                        Subject: string.Format("{0} problem report (version {1})", AppSettings.Instance.AppName, AppSettings.Instance.VersionNumber),
                        Body: loggedError));

            RefreshFeedsAndStartListeningToNewNews();
            InitializeExtraPanoramaItems();

            await Task.Yield();

            this.IsHitTestVisible = true;

            var panoSelectionChanged = Observable.FromEventPattern<SelectionChangedEventArgs>(pano, "SelectionChanged");
            panoSelectionChanged.Where(_ => pano.SelectedItem == Featured_News).Take(1).Subscribe(_ => OnFirstFeaturedNewsPanoItemLoad());
            panoSelectionChanged.Subscribe(_ => OnPanoSelectionChanged());

            this.NavigatedTo.Subscribe(OnSubsequentNavigatedTo);
        }

        async void OnSubsequentNavigatedTo()
        {
            ApplicationBar.IsVisible = (pano.SelectedItem == Categories);
            await vm.LoadSourcesAsync();
        }

        void OnPanoSelectionChanged()
        {
            ApplicationBar.IsVisible = (pano.SelectedItem == Categories);
        }

        async Task OnFirstFeaturedNewsPanoItemLoad()
        {
            await TimeSpan.FromSeconds(0.3);
            this.cat1.NewsItemClicked.Subscribe(ShowDetailed);
            await vm2.RefreshNewsAsync();

            if (currentRefreshListener == null)
                InitializeCurrentRefreshListener();

            await currentRefreshListener.GetRefreshed();
            await vm2.RefreshNewsAsync();
        }

        async Task RefreshFeedsAndStartListeningToNewNews()
        {
            if (AppSettings.Instance.StartupMode == StartupMode.Launch)
            {
                if (!AppSettings.Instance.IsNetworkAvailable)
                    MessageBox.Show("You currently  have no network connection. \r\n\r\nYou can still view the headlines and teasers for previously downloaded articles!", "Cached Mode", MessageBoxButton.OK);
                else
                {
                    progressBar.IsIndeterminate = true;
                    progressBar.Visibility = Visibility.Visible;

                    var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
                    var feeds = await dal.Feeds.Get();

                    FeedSource.NewsServer.BeginFeedUpdateBatch();
                    foreach (var feed in feeds)
                        feed.RefreshNews();
                    FeedSource.NewsServer.EndFeedUpdateBatch();


                    if (currentRefreshListener == null)
                        InitializeCurrentRefreshListener();
                    await currentRefreshListener.GetRefreshed();

                    progressBar.IsIndeterminate = false;
                    progressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        void InitializeCurrentRefreshListener()
        {
            currentRefreshListener = new CategoryRefreshListener("all news");
        }




        #region Page Initialization

        void InitializeExtraPanoramaItems()
        {
            int insertionIndex = 2;
            foreach (var item in PanoramaInjectionService.GetAll())
            {
                pano.Items.Insert(insertionIndex++, item);
            }
        }

        #endregion Initialize Page




        #region Button and event handling (article tapped, category tapped, most viewed tapped, appbar buttons tapped)

        void ShowDetailed(NewsItem newsItem)
        {
            if (newsItem == null)
                return;

            newsItem.HasBeenViewed = true;
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.cat1.LayoutRoot);
            GlobalNavigationService.ToWebBrowserPage(newsItem);
        }

        void OnMostViewedTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.mostViewed);
            ToArticleList(((Button)sender).DataContext as CategoryOrLooseFeedViewModel);
        }

        void OnCategoryTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.categoriesContainer);
            ApplicationBar.IsVisible = false;
            ToArticleList(((Button)sender).DataContext as CategoryOrLooseFeedViewModel);
        }

        void ToArticleList(CategoryOrLooseFeedViewModel o)
        {
            if (o == null)
                return;

            if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category)
            {
                var category = o.Name;
                GlobalNavigationService.ToMainPage(category, "category");
            }
            else if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed)
            {
                var feed = o.Name;
                GlobalNavigationService.ToMainPage(feed, o.FeedId);
            }
        }

        void favoritesButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.menu);
            GlobalNavigationService.ToMainPage("favorites", "favorites");
        }

        void settingsButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.menu);
            GlobalNavigationService.ToAppSettingsPage();
        }

        void moreAppsButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.menu);
            GlobalNavigationService.ToSelesGamesInfoPage();
        }

        void OnManageFeedAppBarButtonClicked(object sender, System.EventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, null);
            NavigationService.ToManageSourcesPage();
        }

        async void OnLoginButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileService = new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
                var mobileUser = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                var userId = mobileUser.UserId;
            }
            catch (InvalidOperationException)
            {
                //message = "You must log in. Login Required";
            }
            var accountClient = new Weave.Services.Account.Client();
            
        }

        #endregion
    }
}