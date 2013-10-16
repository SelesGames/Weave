using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using SelesGames;
using SelesGames.Phone;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;
using Weave.UI.Frame;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;
using Weave.ViewModels.Identity;

namespace weave
{
    public partial class SamplePanorama : WeavePage
    {
        PanoramaViewModel vm;
        IdentityInfo identity;
        OverlayFrame frame;
        UserInfo user;
        bool isIdentityInitialized = false;

        public SamplePanorama()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            frame = ServiceResolver.Get<OverlayFrame>();
            user = ServiceResolver.Get<UserInfo>();
            identity = ServiceResolver.Get<IdentityInfo>();

            identity.UserIdChanged += async (s, e) =>
            {
                bool updateFailed = false;
                try
                {
                    frame.OverlayText = "Updating user...";
                    frame.IsLoading = true;
                    user.Id = identity.UserId;
                    // refresh user news?
                    await user.Load(refreshNews: true);
                }
                catch
                {
                    updateFailed = true;
                }
                if (updateFailed)
                {
                    frame.OverlayText = "Failed to update user";
                    await Task.Delay(2000);
                }
                frame.IsLoading = false;
            };

            vm = new PanoramaViewModel();
            this.DataContext = vm;
            //this.cat1.DataContext = ServiceResolver.Get<IUserCache>().Get();

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

            vm.LoadMostViewedAsync();
            SetValue(RadTransitionControl.TransitionProperty, new RadTileTransition { PlayMode = TransitionPlayMode.Manual });

            mosaicHubTile.CreateImageSource = o => CreateImageSourceFromFeed(o as Feed);
        }

        ImageSource CreateImageSourceFromFeed(Feed feed)
        {
            if (feed == null)
                return null;

            return new BitmapImage(new Uri(feed.TeaserImageUrl, UriKind.Absolute));
        }

        protected async override void OnPageLoad(WeaveNavigationEventArgs navigationEventArgs)
        {
            if (AppSettings.Instance.LogExceptions)
                LittleWatson.LogPreviousExceptionIfPresent(loggedError =>
                    SelesGames.Phone.TaskService.ToEmailComposeTask(
                        To: "info@selesgames.com",
                        Subject: string.Format("{0} problem report (version {1})", AppSettings.Instance.AppName, AppSettings.Instance.VersionNumber),
                        Body: loggedError));

            //RefreshFeedsAndStartListeningToNewNews();

            await Task.Yield();

            this.IsHitTestVisible = true;

            var panoSelectionChanged = Observable.FromEventPattern<SelectionChangedEventArgs>(pano, "SelectionChanged");
            panoSelectionChanged.Where(_ => pano.SelectedItem == Featured_News).Take(1).Subscribe(_ => OnFirstFeaturedNewsPanoItemLoad());
            panoSelectionChanged.Subscribe(_ => OnPanoSelectionChanged());

            InitializeIdentity();

            this.NavigatedTo.Subscribe(OnSubsequentNavigatedTo);
        }

        async void InitializeIdentity()
        {
            if (isIdentityInitialized)
                return;

            isIdentityInitialized = true;

            try
            {
                await identity.LoadFromUserId();
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        void OnSubsequentNavigatedTo()
        {
            mosaicHubTile.IsFrozen = false;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            mosaicHubTile.IsFrozen = true;
            base.OnNavigatedFrom(e);
        }

        void OnPanoSelectionChanged()
        {
            ApplicationBar.Mode = (pano.SelectedItem == Account) ? ApplicationBarMode.Default : ApplicationBarMode.Minimized;
        }

        async Task OnFirstFeaturedNewsPanoItemLoad()
        {
            await TimeSpan.FromSeconds(0.3);
            this.cat1.NewsItemClicked.Subscribe(ShowDetailed);
            //vm.LoadLatestNews();
            this.cat1.DataContext = ServiceResolver.Get<IUserCache>().Get();
        }




        #region Button and event handling (article tapped, category tapped, most viewed tapped, appbar buttons tapped)

        void ShowDetailed(NewsItem newsItem)
        {
            if (newsItem == null)
                return;

            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.cat1.LayoutRoot);
            GlobalNavigationService.ToWebBrowserPage(newsItem);
        }

        void OnMostViewedTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.menuTileWrapper);
            ToArticleList(((Button)sender).DataContext as NewsItemGroup);
        }

        void allSourcesButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.menuTileWrapper);
            GlobalNavigationService.ToMainPage(null, "sources");
        }

        //void OnCategoryTapped(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    //SetValue(RadTileAnimation.ContainerToAnimateProperty, this.categoriesContainer);
        //    ApplicationBar.IsVisible = false;
        //    ToArticleList(((Button)sender).DataContext as CategoryOrLooseFeedViewModel);
        //}

        void ToArticleList(NewsItemGroup o)
        {
            if (o == null)
                return;

            if (o is CategoryGroup)
            {
                var category = (CategoryGroup)o;
                GlobalNavigationService.ToMainPage(category.DisplayName, "category");
            }
            else if (o is FeedGroup)
            {
                var feed = (FeedGroup)o;
                GlobalNavigationService.ToMainPage(feed.Feed);
            }
        }

        void favoritesButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.menu);
            GlobalNavigationService.ToMainPage("favorites", "favorites");
        }

        void readButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, this.menu);
            GlobalNavigationService.ToMainPage("favorites", "favorites");
        }

        void manageSourcesButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, null);
            NavigationService.ToManageSourcesPage();
        }

        void allNewsButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, sender);
            GlobalNavigationService.ToMainPage("all news", "category");
        }

        void OnSettingsAppBarButtonClicked(object sender, System.EventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, null);
            GlobalNavigationService.ToAppSettingsPage();
        }

        void OnInfoAppBarButtonClicked(object sender, System.EventArgs e)
        {
            SetValue(RadTileAnimation.ContainerToAnimateProperty, null);
            GlobalNavigationService.ToSelesGamesInfoPage();
        }

        //void OnLoginButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    GlobalNavigationService.ToAccountSignInPage(); 
        //}

        #endregion




        #region Account login/sync

        MobileServiceClient CreateMobileServiceClient()
        {
            return new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
        }

        async void OnFacebookButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!isIdentityInitialized)
                return;

            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                frame.OverlayText = "Syncing account...";
                frame.IsLoading = true;
                await identity.SyncFacebook(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            frame.IsLoading = false;
        }

        async void OnTwitterButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!isIdentityInitialized)
                return;

            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Twitter);
                frame.OverlayText = "Syncing account...";
                frame.IsLoading = true;
                await identity.SyncTwitter(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            frame.IsLoading = false;
        }

        async void OnMicrosoftButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!isIdentityInitialized)
                return;

            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                frame.OverlayText = "Syncing account...";
                frame.IsLoading = true;
                await identity.SyncMicrosoft(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            frame.IsLoading = false;
        }

        async void OnGoogleButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!isIdentityInitialized)
                return;

            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Google);
                frame.OverlayText = "Syncing account...";
                frame.IsLoading = true;
                await identity.SyncGoogle(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            frame.IsLoading = false;
        }

        #endregion
    }
}