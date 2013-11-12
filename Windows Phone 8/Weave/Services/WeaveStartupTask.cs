using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using weave.Resources;
using weave.Services;
using weave.Services.Startup;
using Weave.FeedLibrary;
using Weave.NinjectKernel;
using Weave.SavedState;
using Weave.UI.Frame;
using Weave.ViewModels;
using Weave.ViewModels.Helpers;
using Weave.ViewModels.Identity;
using Weave.ViewModels.Repository;
using ArticleService = Weave.Article.Service.Client;
using UserService = Weave.User.Service.Client;

namespace weave
{
    public class WeaveStartupTask
    {
        #region private member variables

        AppSettings settings;
        PermanentState permanentState;
        TombstoneState tombstoneState;
        UserInfo user;
        IdentityInfo identity; 
        OverlayFrame frame;
        Kernel kernel;
        DataStorageClient storageClient;

        bool 
            isFrameInit,
            isFirstNavInit,
            isAppLevelExceptionHandlerInitialized,
            isNetworkStatusChangeListenerInit,
            isGConPanoInit,
            isOrientationChangeServiceInit;

        #endregion




        #region Constructor

        public WeaveStartupTask(AppSettings settings)
        {
            this.settings = settings;
            AppSettings.Instance = settings;

            storageClient = new DataStorageClient();

            InitializeApplicationLevelExceptionHandler();
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

        #endregion




        #region Initial Navigation

        async void OnInitialNavigatingWrapper(EventPattern<NavigatingCancelEventArgs> args)
        {
            if (isFirstNavInit)
                return;

            isFirstNavInit = true;

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
            Uri originalTargetUri = null, destinationUri = null;

            originalTargetUri = args.EventArgs.Uri;

            frame.IsHitTestVisible = false;
            frame.OverlayText = "Getting your news...";
            frame.IsLoading = true;


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
            await frame.GetLayoutUpdated().Take(1).ToTask();

            var dummyPage = frame.Content as DummyPage;

            // at this point, the loading screen is being shown

            await InitializePermanentState();
            await InitializeTombstoneState();

            InitializeNetworkStatusChangeListener();
            var isInternetAvailable = await CheckForInternetConnection();
            if (!isInternetAvailable)
            {
                MessageBox.Show("Please check your internet connection and re-open the app", "No internet connection", MessageBoxButton.OK);
                App.Current.Terminate();
                return;
            }

            InitializeUser();

            var stateMachine = new StartupIdentityStateMachine(user);
            try
            {
                await stateMachine.Begin();
            }
            catch
            {
                MessageBox.Show("Unable to initialize user.  Please check your internet connection and re-open the app", "Internet error", MessageBoxButton.OK);
                App.Current.Terminate();
                return;
            }

            InitializeIdentity();

            InitializeAdSettings();
            InitializeNinjectKernel();
            InitializeGarbageCollectionOnNavigateToPanorama();
            InitializeOrientationChangeService();
            InitializeThemes();
            new SystemTrayNavigationSetter(frame, permanentState);


            await dummyPage.LayoutPopups();

            if (stateMachine.FinalState == StartupIdentityStateMachine.State.UserExists)
            {
                destinationUri = originalTargetUri;
            }
            else if (stateMachine.FinalState == StartupIdentityStateMachine.State.NoUserFound)
            {
                destinationUri = new Uri("/weave;component/Pages/SelectTheCategoriesThatInterestYouPage.xaml", UriKind.Relative);
            }

            frame.TryNavigate(destinationUri);
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




        #region Check for Internet Connection

        async Task<bool> CheckForInternetConnection()
        {
            for (int i = 0; i < 3; i++)
            {
                if (settings.IsNetworkAvailable)
                    return true;

                await Task.Delay(100);
            }

            return false;
        }

        #endregion




        #region Initialization functions




        #region Initialize New PhoneApplicationFrame

        void InitializeNewFrame()
        {
            if (isFrameInit)
                return;

            isFrameInit = true;

            frame = new OverlayFrame
            {
                OverlayBackground = new SolidColorBrush(Color.FromArgb(255, 31, 31, 31)),
                OverlayForeground = new SolidColorBrush(Colors.White)
            };
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




        #region Initialize Network Status changes listener

        void InitializeNetworkStatusChangeListener()
        {
            if (isNetworkStatusChangeListenerInit)
                return;

            isNetworkStatusChangeListenerInit = true;

            NetworkChange.NetworkAddressChanged += (s, e) =>
            {
                settings.IsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            };
            settings.IsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        }

        #endregion




        #region Initialize App-Level Exception Handler

        void InitializeApplicationLevelExceptionHandler()
        {
            if (isAppLevelExceptionHandlerInitialized)
                return;

            isAppLevelExceptionHandlerInitialized = true;

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
        }

        #endregion




        #region Initialize PermanentState

        async Task InitializePermanentState()
        {
            if (permanentState != null)
                return;

            permanentState = await storageClient.GetPermanentState();
            if (settings.StartupMode == StartupMode.Launch)
                permanentState.RunHistory.CreateNewLog();
        }

        #endregion




        #region Initialize TombstoneState

        async Task InitializeTombstoneState()
        {
            if (settings.StartupMode == StartupMode.Launch)
                tombstoneState = new TombstoneState();
            else
                tombstoneState = await storageClient.GetTombstoneState();
        }

        #endregion




        #region Initialize User

        void InitializeUser()
        {
            if (user != null)
                return;

            var repo = new StandardRepository(new UserService.Client(), new ArticleService.ServiceClient());
            user = new UserInfo(repo);
            user.Id = permanentState.UserId;
            user.PropertyChanged += OnUserInfoUserIdChanged;
        }

        void OnUserInfoUserIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Id")
            {
                permanentState.UserId = user.Id;
            }
        }

        #endregion




        #region Initialize Identity

        void InitializeIdentity()
        {
            if (identity != null)
                return;

            identity = new IdentityInfo(new Weave.Identity.Service.Client.ServiceClient()) { UserId = user.Id };
            identity.UserIdChanged += OnIdentityUserIdChanged;
        }

        async void OnIdentityUserIdChanged(object sender, EventArgs e)
        {
            await Task.Yield();
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
        }

        #endregion




        #region Initialize Ad Settings

        void InitializeAdSettings()
        {
            SelesGames.UI.Advertising.AdSettings.IsAddSupportedApp = settings.IsAddSupportedApp;
        }

        #endregion




        #region Initialize Ninject Kernel

        void InitializeNinjectKernel()
        {
            if (kernel != null)
                return;

            kernel = new Kernel(settings.AssemblyName);
            ServiceResolver.SetInternalResolver(new NinjectToServiceResolverAdapter(kernel));

            kernel.Bind<SocialShareContextMenuControl>().ToSelf().InSingletonScope().Named("accent")
                .OnActivation((_, o) =>
                {
                    o.HideCloseButtonForAppBarSetup();
                    o.Background = settings.Themes.CurrentTheme.AccentBrush;
                });
            kernel.Bind<SelesGames.UI.Advertising.Common.AdSettingsClient>().ToMethod(_ =>
                new SelesGames.UI.Advertising.Common.AdSettingsClient(settings.AdUnitsUrl))
                .InSingletonScope();
            kernel.Bind<PermanentState>().ToConstant(permanentState).InSingletonScope();
            kernel.Bind<TombstoneState>().ToConstant(tombstoneState).InSingletonScope();
            kernel.Bind<UserInfo>().ToConstant(user).InSingletonScope();
            kernel.Bind<IdentityInfo>().ToConstant(identity).InSingletonScope();
            kernel.Bind<BindableMainPageFontStyle>().ToSelf().InSingletonScope();
            kernel.Bind<FontSizePopup>().ToSelf().InSingletonScope();
            kernel.Bind<FontAndThemePopup>().ToSelf().InSingletonScope();
            kernel.Bind<ExpandedLibrary>().ToMethod(_ => new ExpandedLibrary(AppSettings.Instance.ExpandedFeedLibraryUrl)).InSingletonScope();
            kernel.Bind<ViewModelLocator>().ToSelf().InSingletonScope();
            kernel.Bind<FeedsToNewsItemGroupAdapter>().ToConstant(new FeedsToNewsItemGroupAdapter(user)).InSingletonScope();
            kernel.Bind<OverlayFrame>().ToConstant(frame).InSingletonScope();
            kernel.Bind<PhoneApplicationFrame>().ToConstant(frame).InSingletonScope();
        }

        #endregion




        #region Initialize GarbageCollection handler when navigating to Panorama (home page)

        void InitializeGarbageCollectionOnNavigateToPanorama()
        {
            if (isGConPanoInit)
                return;

            isGConPanoInit = true;

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

        #endregion




        #region Initialze Orientation Change Service

        void InitializeOrientationChangeService()
        {
            if (isOrientationChangeServiceInit)
                return;

            isOrientationChangeServiceInit = true;

            var orientationService = new OrientationLockService(frame);
            orientationService.Start();
            kernel.Bind<OrientationLockService>().ToConstant(orientationService).InSingletonScope();
        }

        #endregion




        #region Initialize Themes

        void InitializeThemes()
        {

            if (settings.Themes != null)
                return;

            settings.Themes = new StandardThemeSet(settings.CurrentApplication, permanentState);
            settings.Themes.UpdateCurrentThemeFromPermanentState();
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
                await storageClient.Save(tombstoneState);
                DebugEx.WriteLine("******************* SAVED TOMBSTONESTATE");
                await storageClient.Save(permanentState);
                DebugEx.WriteLine("******************* SAVED PERMANENTSTATE");
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