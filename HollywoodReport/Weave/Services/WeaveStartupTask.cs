using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using weave.Data;
using Weave.FeedLibrary;
using Weave.NinjectKernel;

namespace weave
{
    public class WeaveStartupTask
    {
        AppSettings settings;
        PermanentState permanentState;
        CustomFrame frame;
        Kernel kernel;
        Weave4DataAccessLayer dataAccessLayer;
        bool hasBeenInitialized = false;

        public WeaveStartupTask(AppSettings settings)
        {
            this.settings = settings;
            AppSettings.Instance = settings;

            InitializeNewFrame();

            var phoneAppService = PhoneApplicationService.Current;
            phoneAppService.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;

            phoneAppService.Launching += (s, e) => OnLaunching();
            phoneAppService.Activated += (s, e) => OnActivated();
            phoneAppService.Deactivated += (s, e) => Stop();
            phoneAppService.Closing += (s, e) => Stop();
        }




        #region Initialize the new PhoneApplicationFrame

        void InitializeNewFrame()
        {
            frame = new CustomFrame();
            GlobalNavigationService.CurrentFrame = frame;

            Observable
                .FromEventPattern<NavigationEventArgs>(frame, "Navigated")
                .Take(1)
                .Subscribe(_ => settings.CurrentApplication.RootVisual = frame);

            Observable
               .FromEventPattern<NavigatingCancelEventArgs>(frame, "Navigating")
               .Take(1)
               .Subscribe(OnInitialNavigating);

            new ArticleListNavigationCorrector(frame);
            frame.Navigating += (s, e) => frame.IsHitTestVisible = false;
            frame.Navigated += (s, e) => frame.IsHitTestVisible = true;
        }

        async void OnInitialNavigating(EventPattern<NavigatingCancelEventArgs> args)
        {
            new SystemTrayNavigationSetter(frame, permanentState);

            if (permanentState.IsFirstTime)
            {
                frame.IsLoading = false;
                dataAccessLayer = ServiceResolver.Get<Weave4DataAccessLayer>();
                dataAccessLayer.IsFirstTime = true;
                return;
            }

            frame.IsHitTestVisible = false;

            var originalTargetUri = args.EventArgs.Uri;

            bool shouldReroutNavigation = args.EventArgs.IsCancelable;
            int backStackRemovalCount = 0;

            if (shouldReroutNavigation)
            {
                backStackRemovalCount = 1;
                args.EventArgs.Cancel = true;

                await Observable.FromEventPattern<NavigationEventArgs>(frame, "NavigationStopped").Take(1).ToTask();

                frame.Navigate(new Uri("/weave;component/Pages/DummyPage.xaml", UriKind.Relative));
            }
            else
            {
                backStackRemovalCount = 2;
                await Observable.FromEventPattern<NavigationEventArgs>(frame, "Navigated").Take(1).ToTask();
                frame.Navigate(new Uri("/weave;component/Pages/DummyPage.xaml", UriKind.Relative));
            }

            await Observable.FromEventPattern<NavigationEventArgs>(frame, "Navigated").Take(1).ToTask();

            frame.IsLoading = true;

            await frame.GetLayoutUpdated().Take(1).ToTask();

            var dummyPage = frame.Content as DummyPage;
            await dummyPage.LayoutPopups();

            dataAccessLayer = ServiceResolver.Get<Data.Weave4DataAccessLayer>();

            var markedReadTimes = new ArticleDeleteTimesForMarkedRead();
            var unreadTimes = new ArticleDeleteTimesForUnread();

            dataAccessLayer.OldMarkedReadNewsElapsedThreshold = markedReadTimes.GetByDisplayName(permanentState.ArticleDeletionTimeForMarkedRead).Span;
            dataAccessLayer.OldUnreadNewsElapsedThreshold = unreadTimes.GetByDisplayName(permanentState.ArticleDeletionTimeForUnread).Span;

            bool wereFeedsRecovered = false;
            bool areThereTooManyFeeds = false;
            try
            {
                var feeds = await dataAccessLayer.Feeds.Get();
                wereFeedsRecovered = true;
                areThereTooManyFeeds = feeds.AreThereTooManyFeeds();
            }
            catch { }

            if (!wereFeedsRecovered)
            {
                var result = MessageBox.Show("We apologize for this, but we are unable to recover your categories and sources from your phone's memory.  Tap the back button twice to exit the app.  The next time you start, you can reselect your categories and sources.", "SERIOUS ERROR", MessageBoxButton.OK);
                permanentState.IsFirstTime = true;
                return;
            }

            if (areThereTooManyFeeds)
            {
                frame.Navigate(new Uri("/Weave.UI.SettingsPages;component/Views/ManageSourcesPage.xaml", UriKind.Relative));
            }
            else
            {
                frame.Navigate(originalTargetUri);
            }
            await Observable.FromEventPattern<NavigationEventArgs>(frame, "Navigated").Take(1).ToTask();

            var page = frame.Content as PhoneApplicationPage;
            var pageLayoutComplete = page.GetLayoutUpdated().Take(1).ToTask();

            while (frame.BackStack.Count() > 0 && backStackRemovalCount-- > 0)
                frame.RemoveBackEntry();

            await pageLayoutComplete;

            frame.IsLoading = false;

            frame.IsHitTestVisible = true;
        }

        #endregion




        async Task OnLaunching()
        {
            settings.StartupMode = StartupMode.Launch;

            await RecoverPermanentStateAsync();

            permanentState.PreviousLoginTime = permanentState.CurrentLoginTime;
            permanentState.CurrentLoginTime = DateTime.UtcNow;
            settings.LastLoginTime = permanentState.PreviousLoginTime;
            permanentState.RunHistory.CreateNewLog();

            if (permanentState.IsFirstTime && settings.CanSelectInitialCategories)
            {
                FinishInitialization();
                GlobalNavigationService.ToWelcomePage();
            }
            else
            {
                FinishInitialization();
                GlobalNavigationService.ToPanoramaPage();
            }
        }

        async Task OnActivated()
        {
            settings.StartupMode = StartupMode.Activate;

            if (!hasBeenInitialized)
            {
                await RecoverPermanentStateAsync();
                settings.LastLoginTime = permanentState.PreviousLoginTime;
                FinishInitialization();
            }
            else
                settings.LastLoginTime = permanentState.PreviousLoginTime;
        }

        async Task RecoverPermanentStateAsync()
        {
            permanentState = await settings.PermanentState.Get();
        }




        #region finish Initialization of the app

        void FinishInitialization()
        {
            settings.Themes = new StandardThemeSet(settings.CurrentApplication, permanentState);
            settings.Themes.UpdateCurrentThemeFromPermanentState();

            settings.CurrentApplication.UnhandledException += (s, e) =>
            {
                if (settings.LogExceptions)
                    LittleWatson.ReportException(e.ExceptionObject, string.Empty);
            };


            settings.IsNetworkAvailable = Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            NetworkChange.NetworkAddressChanged += (s, e) =>
            {
                settings.IsNetworkAvailable = Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            };


            if (settings.StartupMode == StartupMode.Launch)
                settings.TombstoneState.IgnoreIsolatedStorage = true;

            InitializeAdSettings();
            InitializeNinjectKernel();
            InitializeGarbageCollectionOnNavigateToPanorama();
            InitializeOrientationChangeService();
            hasBeenInitialized = true;
        }

        void InitializeAdSettings()
        {
            weave.UI.Advertising.AdSettings.AdApplicationId = settings.AdApplicationId;
            weave.UI.Advertising.AdSettings.IsAddSupportedApp = settings.IsAddSupportedApp;
            var adCollection = new weave.UI.Advertising.AdUnitCollection("http://weavestorage.blob.core.windows.net/settings/adunits", "31892");
        }

        void InitializeNinjectKernel()
        {
            kernel = new Kernel(settings.AssemblyName);

            kernel.Bind<MainPageSettingsPopup>().ToSelf().InSingletonScope();
            kernel.Bind<BindableMainPageFontStyle>().ToSelf().InSingletonScope();

            kernel.Bind<SocialShareContextMenuControl>().ToSelf().InSingletonScope().Named("accent")
                .OnActivation((_, o) =>
                {
                    o.HideCloseButtonForAppBarSetup();
                    o.Background = settings.Themes.CurrentTheme.AccentBrush;
                });
            kernel.Bind<FontSizePopup>().ToSelf().InSingletonScope();
            kernel.Bind<ExpandedLibrary>().ToMethod(_ => new ExpandedLibrary(AppSettings.Instance.ExpandedFeedLibraryUrl)).InSingletonScope();

            kernel.Bind<MainPageNavigationDropDownList>().ToSelf().InSingletonScope();
            ServiceResolver.SetInternalResolver(new NinjectToServiceResolverAdapter(kernel));
        }

        void InitializeGarbageCollectionOnNavigateToPanorama()
        {
            Observable
                .FromEventPattern<NavigationEventArgs>(frame, "Navigated")
                .Where(e =>
                    e.EventArgs != null &&
                    e.EventArgs.Content != null &&
                    e.EventArgs.Content.GetType() == typeof(SamplePanorama))
                .Subscribe(() =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });
        }

        void InitializeOrientationChangeService()
        {
            var orientationService = new OrientationLockService(frame);
            orientationService.Start();
            kernel.Bind<OrientationLockService>().ToConstant(orientationService).InSingletonScope();
        }

        #endregion




        #region Stop the service

        void Stop()
        {
            var task = TaskEx.Run(() => StopAsync().Wait());

            var timeIncrement = TimeSpan.FromSeconds(0.1);
            var counter = TimeSpan.FromSeconds(0);
            while (counter < TimeSpan.FromSeconds(6))
            {
                if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                    return;

                Thread.Sleep(timeIncrement);
                counter += timeIncrement;
            }
        }

        async Task StopAsync()
        {
            permanentState.CurrentLoginTime = DateTime.UtcNow;

            bool stoppedSuccessfully = false;
            try
            {
                DebugEx.WriteLine("******************* STOPPING WEAVE");
                await AppSettings.Instance.TombstoneState.Save();
                DebugEx.WriteLine("******************* SAVED TOMBSTONESTATE");
                await AppSettings.Instance.PermanentState.Save();
                DebugEx.WriteLine("******************* SAVED PERMANENTSTATE");
                                
                var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
                await dal.SaveOnExit();
                DebugEx.WriteLine("******************* SAVED FEEDS AND NEWS");

                stoppedSuccessfully = true;
            }
            catch(Exception e)
            {
                DebugEx.WriteLine("Exception on exit: {0}", e);
            }
            if (stoppedSuccessfully)
                DebugEx.WriteLine("******************* PROGRAM EXITED CLEANLY!");
        }

        #endregion
    }
}