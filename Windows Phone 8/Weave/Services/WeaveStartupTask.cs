using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using weave.Resources;
using weave.Services;
using weave.Services.Startup;
using Weave.FeedLibrary;
using Weave.NinjectKernel;
using Weave.SavedState;
using Weave.UI.Frame;
using Weave.ViewModels;
using Weave.ViewModels.Cache;
using Weave.ViewModels.Contracts.Client;
using Weave.ViewModels.Helpers;
using Weave.ViewModels.Identity;
using Weave.ViewModels.Repository;

namespace weave
{
    public class WeaveStartupTask
    {
        AppSettings settings;
        PermanentState permanentState;
        OverlayFrame frame;
        Kernel kernel;
        //Weave4DataAccessLayer dataAccessLayer;
        bool isFullyInitialized = false;
        Uri initialNavigationUri = null;
        StandardUserCache userCache;

        public WeaveStartupTask(AppSettings settings)
        {
            this.settings = settings;
            AppSettings.Instance = settings;

            InitializeNewFrame();
            InitializeLanguage();

            var phoneAppService = PhoneApplicationService.Current;
            phoneAppService.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;

            phoneAppService.Launching += (s, e) => settings.StartupMode = StartupMode.Launch;
            phoneAppService.Activated += (s, e) => settings.StartupMode = StartupMode.Activate;
            phoneAppService.Deactivated += (s, e) => Stop();
            phoneAppService.Closing += (s, e) => Stop();

            EnableLiveTileUpdatingBackgroundTask();
        }




        #region Initialize the new PhoneApplicationFrame

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        void InitializeNewFrame()
        {
            if (phoneApplicationInitialized)
                return;

            frame = new OverlayFrame();
            GlobalNavigationService.CurrentFrame = frame;

            Observable
                .FromEventPattern<NavigationEventArgs>(frame, "Navigated")
                .Take(1)
                .Subscribe(_ => settings.CurrentApplication.RootVisual = frame);

            Observable
               .FromEventPattern<NavigatingCancelEventArgs>(frame, "Navigating")
               .Take(1)
               .Subscribe(OnInitialNavigatingWrapper);

            new ArticleListNavigationCorrector(frame);
            frame.Navigating += (s, e) => frame.IsHitTestVisible = false;
            frame.Navigated += (s, e) => frame.IsHitTestVisible = true;
            frame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        #endregion




        #region On first Navigation event

        async void OnInitialNavigatingWrapper(EventPattern<NavigatingCancelEventArgs> args)
        {
            try
            {
                await OnInitialNavigating(args);
            }
            catch (Exception ex)
            {
                // these sort of exceptions are ok to ignore
                if (ex.Message == "Navigation is not allowed when the task is not in the foreground.")
                    return;
                else
                    throw ex;
            }
        }


        async Task OnInitialNavigating(EventPattern<NavigatingCancelEventArgs> args)
        {
            initialNavigationUri = args.EventArgs.Uri;

            ClearUpdateCountOnAllTiles();

            frame.IsHitTestVisible = false;
            frame.IsLoading = true;

            var originalTargetUri = args.EventArgs.Uri;

            bool shouldReroutNavigation = args.EventArgs.IsCancelable;
            int backStackRemovalCount = 0;

            if (shouldReroutNavigation)
            {
                backStackRemovalCount = 1;
                args.EventArgs.Cancel = true;

                await frame.NavigationStoppedAsync();
            }
            else
            {
                backStackRemovalCount = 2;
                await frame.NavigatedAsync();
            }

            frame.TryNavigate("/weave;component/Pages/DummyPage.xaml");
            await frame.NavigatedAsync();

            //frame.IsLoading = true;

            await frame.GetLayoutUpdated().Take(1).ToTask();

            var dummyPage = frame.Content as DummyPage;


            // at this point, the loading screen is being shown

            if (permanentState == null)
                permanentState = await new DataStorageClient().Get<PermanentState>();// settings.PermanentState.Get();

            new SystemTrayNavigationSetter(frame, permanentState);

            if (settings.StartupMode == StartupMode.Launch)
            {
                permanentState.RunHistory.CreateNewLog();
            }

            FinishInitialization();

            await dummyPage.LayoutPopups();


            var user = userCache.Get();
            var stateMachine = new StartupIdentityStateMachine(user);
            await stateMachine.Begin();

            if (stateMachine.FinalState == StartupIdentityStateMachine.State.UserExists)
            {
                originalTargetUri = new Uri("/weave;component/Pages/Panorama/SamplePanorama.xaml", UriKind.Relative);
            }
            else if (stateMachine.FinalState == StartupIdentityStateMachine.State.NoUserFound)
            {
                originalTargetUri = new Uri("/weave;component/Pages/SelectTheCategoriesThatInterestYouPage.xaml", UriKind.Relative);
            }

            //dataAccessLayer = ServiceResolver.Get<Data.Weave4DataAccessLayer>();

            frame.TryNavigate(originalTargetUri);
            await frame.NavigatedAsync();

            var page = frame.Content as PhoneApplicationPage;
            var pageLayoutComplete = page.GetLayoutUpdated().Take(1).ToTask();

            while (frame.BackStack.Count() > 0 && backStackRemovalCount-- > 0)
                frame.RemoveBackEntry();

            await pageLayoutComplete;

            frame.IsLoading = false;
            frame.IsHitTestVisible = true;
        }

        #endregion




        #region Initialize the language support

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                frame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                frame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }

        #endregion




        #region finish Initialization of the app

        void FinishInitialization()
        {
            if (isFullyInitialized)
                return;

            isFullyInitialized = true;

            settings.Themes = new StandardThemeSet(settings.CurrentApplication, permanentState);
            settings.Themes.UpdateCurrentThemeFromPermanentState();

            settings.CurrentApplication.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject;

                if (ex == null)
                    return;

                if (ex is InvalidOperationException && ex.Message == "Navigation is not allowed when the task is not in the foreground.")
                    return;

                if (settings.LogExceptions)
                    LittleWatson.LogException(ex, string.Empty);
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
        }

        void InitializeAdSettings()
        {
            SelesGames.UI.Advertising.AdSettings.IsAddSupportedApp = settings.IsAddSupportedApp;
        }

        void InitializeNinjectKernel()
        {
            kernel = new Kernel(settings.AssemblyName);
            ServiceResolver.SetInternalResolver(new NinjectToServiceResolverAdapter(kernel));

            //kernel.Bind<MainPageSettingsPopup>().ToSelf().InSingletonScope();
            kernel.Bind<BindableMainPageFontStyle>().ToSelf().InSingletonScope();

            kernel.Bind<SocialShareContextMenuControl>().ToSelf().InSingletonScope().Named("accent")
                .OnActivation((_, o) =>
                {
                    o.HideCloseButtonForAppBarSetup();
                    o.Background = settings.Themes.CurrentTheme.AccentBrush;
                });
            kernel.Bind<FontSizePopup>().ToSelf().InSingletonScope();
            kernel.Bind<FontAndThemePopup>().ToSelf().InSingletonScope();

            kernel.Bind<ExpandedLibrary>().ToMethod(_ => new ExpandedLibrary(AppSettings.Instance.ExpandedFeedLibraryUrl)).InSingletonScope();

            //kernel.Bind<MainPageNavigationDropDownList>().ToSelf().InSingletonScope();

            kernel.Bind<SelesGames.UI.Advertising.Common.AdSettingsClient>().ToMethod(_ =>
                new SelesGames.UI.Advertising.Common.AdSettingsClient(settings.AdUnitsUrl))
                .InSingletonScope();

            var user = new UserInfo(
                    new StandardRepository(
                        new Weave.User.Service.Client.Client(),
                        new Weave.Article.Service.Client.ServiceClient()
                    )
                );

            user.Id = permanentState.UserId;

            kernel.Bind<UserInfo>().ToConstant(user).InSingletonScope();

            var identity = new IdentityInfo(new Weave.Identity.Service.Client.ServiceClient()) { UserId = user.Id };
            kernel.Bind<IdentityInfo>().ToConstant(identity).InSingletonScope();

            userCache = new StandardUserCache(user);

            user.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Id")
                {
                    permanentState.UserId = user.Id;
                }
            };

            var feedsListener = new FeedsToNewsItemGroupAdapter(user);

            kernel.Bind<IUserCache>().ToConstant(userCache).InSingletonScope();

            kernel.Bind<ViewModelLocator>().ToSelf().InSingletonScope();
            kernel.Bind<FeedsToNewsItemGroupAdapter>().ToConstant(feedsListener).InSingletonScope();

            kernel.Bind<OverlayFrame>().ToConstant(frame).InSingletonScope();
            kernel.Bind<PhoneApplicationFrame>().ToConstant(frame).InSingletonScope();
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




        #region Turn on Background Task that updates the Live Tiles

        void EnableLiveTileUpdatingBackgroundTask()
        {
            var appName = settings.AppName;
            var taskService = new PeriodicTaskService(string.Format("pts:{0}", appName.ToUpperInvariant()))
            {
                Description = string.Format(
#if DEBUG
                "DEMO LIVE TILE WEAVE UPDATING.", appName)
#else
                "Enables *** LIVE TILE *** updating for {0}.  If you disable this, you will lose Live Tiles for this app.", appName)
#endif
            };
            var regResult = taskService.TryRegister();
            if (!regResult)
                DebugEx.WriteLine(taskService.RegistrationException);
        }

        void ClearUpdateCountOnAllTiles()
        {
            if (initialNavigationUri == null)
                return;

            var tiles = (IEnumerable<ShellTile>)ShellTile.ActiveTiles.ToList();

            if (!initialNavigationUri.Equals(new Uri("/weave;component/Pages/Panorama/SamplePanorama.xaml", UriKind.Relative)))
                tiles = tiles.Where(o => o.NavigationUri.Equals(initialNavigationUri));

            foreach (var tile in tiles)
            {
                var tileData = new CycleTileData { Count = 0 };
                tile.TryUpdate(tileData);
            }
        }

        #endregion




        #region Stop the service

        void Stop()
        {
            var task = Task.Run(() => StopAsync().Wait());

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
            bool stoppedSuccessfully = false;
            try
            {
                DebugEx.WriteLine("******************* STOPPING WEAVE");
                await AppSettings.Instance.TombstoneState.Save();
                DebugEx.WriteLine("******************* SAVED TOMBSTONESTATE");
                await AppSettings.Instance.PermanentState.Save();
                DebugEx.WriteLine("******************* SAVED PERMANENTSTATE");

                //var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
                //await dal.SaveOnExit();
                DebugEx.WriteLine("******************* SAVED FEEDS AND NEWS");

                stoppedSuccessfully = true;
            }
            catch (Exception e)
            {
                DebugEx.WriteLine("Exception on exit: {0}", e);
            }
            if (stoppedSuccessfully)
                DebugEx.WriteLine("******************* PROGRAM EXITED CLEANLY!");
        }

        #endregion
    }
}