using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using SelesGames.UI.Advertising;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;


namespace weave
{
    public partial class MainPage : PhoneApplicationPage, IDisposable
    {
        bool isPageInitialized = false;
        string header = null;
        string mode = null;
        MainPageViewModel vm;

        ImageCache imageCache;
        ScrollViewer currentListBoxScroller;
        TrialModeAdControl adControl;
        MainPageNavigationDropDownList jumpList;
        ApplicationBarIconButton refreshButton, fontButton, markPageReadButton;
        ApplicationBarMenuItem lockOrientationButton, openNavMenuButton, pinToStartScreenButton; 
        CompositeDisposable pageLevelDisposables = new CompositeDisposable();




        #region Constructor

        public MainPage()
        {
            InitializeComponent();

            if (DesignerProperties.IsInDesignTool)
                return;

            markPageReadButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            refreshButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            fontButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            lockOrientationButton = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            pinToStartScreenButton = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            openNavMenuButton = ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;

            BindIsOrientationLockedToAppBar();
            this.Loaded += OnLoaded;
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition());

            var permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();
            var isAppBarMinimized = permState.IsHideAppBarOnArticleListPageEnabled;
            ApplicationBar.Mode = isAppBarMinimized ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;
            bottomBarFill.Height = isAppBarMinimized ? 30d : 72d;

            if (OSThemeHelper.GetCurrentTheme() == OSTheme.Light)
                fade.Fill = Resources["LightThemeFade"] as System.Windows.Media.Brush;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged += OnOrientationChanged;
            this.Unloaded += OnUnloaded;
            ChangeBrowserMarginsByPageLayout();
        }

        // Remove event handlers on page unloaded
        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged -= OnOrientationChanged;
            this.Unloaded -= OnUnloaded;
        }

        // change the margins on the webBrowser on page orientation changed
        void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            ChangeBrowserMarginsByPageLayout();
        }

        void ChangeBrowserMarginsByPageLayout()
        {
            //DebugEx.WriteLine("orientation: {0}", Orientation.ToString());
            if (Orientation.IsAnyPortrait())
            {
                bottomBarFill.Visibility = Visibility.Visible;
                leftBarFill.Visibility = Visibility.Collapsed;
                rightBarFill.Visibility = Visibility.Collapsed;
                TitlePanel.Visibility = Visibility.Visible;
                MinTitlePanel.Visibility = Visibility.Collapsed;
            }
            else if (Orientation == PageOrientation.LandscapeLeft)
            {
                rightBarFill.Visibility = Visibility.Visible;
                leftBarFill.Visibility = Visibility.Collapsed;
                bottomBarFill.Visibility = Visibility.Collapsed;
                TitlePanel.Visibility = Visibility.Collapsed;
                MinTitlePanel.Visibility = Visibility.Visible;
            }
            else if (Orientation == PageOrientation.LandscapeRight)
            {
                leftBarFill.Visibility = Visibility.Visible;
                rightBarFill.Visibility = Visibility.Collapsed;
                bottomBarFill.Visibility = Visibility.Collapsed;
                TitlePanel.Visibility = Visibility.Collapsed;
                MinTitlePanel.Visibility = Visibility.Visible;
            }
        }

        void BindIsOrientationLockedToAppBar()
        {
            var orientationLockService = ServiceResolver.Get<OrientationLockService>();

            var bindingAdapter = new ApplicationBarToggleMenuItemAdapter(lockOrientationButton)
            {
                CheckedText = "turn off phone tilt helper",
                UncheckedText = "turn on phone tilt helper",
                IsChecked = orientationLockService.IsLocked,
            };
            bindingAdapter.SetBinding(ApplicationBarToggleMenuItemAdapter.IsCheckedProperty,
                 new Binding("IsLocked") { Source = orientationLockService, Mode = BindingMode.TwoWay });
            bindingAdapter.DisposeWith(pageLevelDisposables);
        }

        #endregion




        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                if (isPageInitialized)
                {
                    ZoomInSB.Stop();
                    if (vm != null)
                    {
                        //var sw = System.Diagnostics.Stopwatch.StartNew();
                        await this.GetLoaded().Take(1).ToTask();
                        await Task.Run(() => vm.OnNavigatedTo());
                        //sw.Stop();
                        //DebugEx.WriteLine("onnavto mvvm {0} ms", sw.ElapsedMilliseconds);
                    }
                }
                else
                {
                    isPageInitialized = true;

                    Guid? feedId = null;

                    if (NavigationContext.QueryString.ContainsKey("header"))
                    {
                        header = NavigationContext.QueryString["header"];
                        panningTitle.Content = header;
                    }
                    if (NavigationContext.QueryString.ContainsKey("mode"))
                    {
                        mode = NavigationContext.QueryString["mode"];
                    }
                    if (NavigationContext.QueryString.ContainsKey("feedId"))
                    {
                        mode = "feed";
                        feedId = Guid.Parse(NavigationContext.QueryString["feedId"]);
                    }

                    var permstate = AppSettings.Instance.PermanentState.Get().WaitOnResult();
                    var tallyer = permstate.RunHistory.GetActiveLog();
                    tallyer.Tally(header);

                    this.vm = new MainPageViewModel(this, header);
                    if (mode.Equals("category", StringComparison.OrdinalIgnoreCase))
                        vm.currentOperatingMode = weave.MainPageViewModel.OperatingMode.Category;
                    if (mode.Equals("feed", StringComparison.OrdinalIgnoreCase))
                    {
                        vm.currentOperatingMode = weave.MainPageViewModel.OperatingMode.Feed;
                        vm.FeedId = feedId.Value;
                    }
                    if (mode.Equals("favorites", StringComparison.OrdinalIgnoreCase))
                        vm.currentOperatingMode = weave.MainPageViewModel.OperatingMode.Favorites;

                    DataContext = this.vm;

                    await Task.Yield();

                    FinishPageInitialization();

                    await TimeSpan.FromSeconds(0.4);

                    await vm.InitializeAsync();
                    vm.AutoRefresh();
                }
            }
            catch (Exception exception)
            {
                DebugEx.WriteLine("exception in MainPage onnavto, {0}", exception);
                NavigationService.SafelyGoBackIfPossible();
            }
        }

        void FinishPageInitialization()
        {
            InitializeAdControl();
            InitializeCustomListAndImageCache();
            InitializeButtonEventHandlers();
            InitializeJumpList();
            pinToStartScreenButton.IsEnabled = IsPinToStartButtonEnabled();
        }




        #region Initialize the Ad Control when necessary

        async void InitializeAdControl()
        {
            if (!AdVisibilityService.AreAdsStillBeingShownAtAll)
                return;

            adControl = new TrialModeAdControl(this.header);
            //adControl.AdMargin = new Thickness(0);
            //adControl.AdHeight = 80;
            //adControl.AdWidth = 480;
            adControl.SetValue(Grid.RowProperty, 0);
            adControl.SetValue(Grid.ColumnProperty, 1);
            adControl.PlayAnimations = true;
            adControl.Opacity = 0;

            LayoutRoot.Children.Add(adControl);
            await TimeSpan.FromSeconds(1);
            adControl.Fade().From(0).To(1).Over(TimeSpan.FromSeconds(0.7)).ToStoryboard().Begin();
        }

        #endregion




        #region Initialize Custom List and Image Cache
        
        void InitializeCustomListAndImageCache()
        {
            this.cl.CompleteInitialization();

            this.imageCache = new ImageCache();
            this.cl.SetImageCache(imageCache);

            this.currentListBoxScroller = cl.scroller;

            SubscribeToNewsItemClicked();
        }

        #endregion




        #region Initialize all button handlers on page (refresh, next/previous page, font, mark page read)

        void InitializeButtonEventHandlers()
        {
            refreshButton.GetClick().Where(_ => vm != null).Subscribe(() => vm.ManualRefresh()).DisposeWith(pageLevelDisposables);
            fontButton.GetClick().Subscribe(LaunchLocalSettingsPopup).DisposeWith(pageLevelDisposables);
            markPageReadButton.GetClick().Subscribe(OnAllRead).DisposeWith(pageLevelDisposables);
            pinToStartScreenButton.GetClick().Subscribe(OnPinToStartButtonPressed).DisposeWith(pageLevelDisposables);
            openNavMenuButton.GetClick().Subscribe(ShowMenu).DisposeWith(pageLevelDisposables);
        }

        #endregion




        #region Initialize Jump List for navigation to other categories

        async Task InitializeJumpList()
        {
            jumpList = ServiceResolver.Get<MainPageNavigationDropDownList>();
            await jumpList.RefreshCategories();
            jumpList.HighlightCurrentCategory(this.vm.Header);
        }

        #endregion




        #region DropDown menu event handling

        void OnJumpListButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ShowMenu();
        }

        void ShowMenu()
        {
            var popup = new SelesGames.PopupService<CategoryOrLooseFeedViewModel>(jumpList);
            popup.BeginShow();
        }

        #endregion




        #region NewsItem click and dismissal handling

        void SubscribeToNewsItemClicked()
        {
            cl.NewsItemSelected
                .Subscribe(OnNewsItemClicked)
                .DisposeWith(pageLevelDisposables);
        }

        async void OnNewsItemClicked(System.Tuple<object, NewsItem> tup)
        {
            var newsItem = (NewsItem)tup.Item2;
            IsHitTestVisible = false;
            ZoomInSB.Begin();
            await Task.Delay(TimeSpan.FromSeconds(0.18d));
            GlobalNavigationService.ToWebBrowserPage(newsItem);
            IsHitTestVisible = true;
            newsItem.HasBeenViewed = true;
        }

        async void OnNewsItemClicked(NewsItem newsItem)
        {
            IsHitTestVisible = false;
            ZoomInSB.Begin();
            await Task.Delay(TimeSpan.FromSeconds(0.18d));
            GlobalNavigationService.ToWebBrowserPage(newsItem);
            IsHitTestVisible = true;
            newsItem.HasBeenViewed = true;
        }

        void OnListItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var context = (sender as FrameworkElement).DataContext as NewsItem;
            if (context != null)
                OnNewsItemClicked(context);
        }

        #endregion




        #region Mark All Read

        void OnAllRead()
        {
            if (vm != null)
                vm.MarkCurrentPageRead();
        }

        #endregion




        #region Pin to Start

        bool IsPinToStartButtonEnabled()
        {
            var currentSource = this.NavigationService.CurrentSource;
            return !ShellTile.ActiveTiles.Any(x => x.NavigationUri.Equals(currentSource));
        }

        async void OnPinToStartButtonPressed()
        {
            if (vm == null)
                return;

            try
            {
                var currentSource = this.NavigationService.CurrentSource;

                // Look to see if the tile already exists and if so, don't try to create again.
                ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.Equals(currentSource));

                // Create the tile if we didn't find it already exists.
                if (TileToFind == null)
                {
                    var liveTileVM = await vm.CreateLiveTileViewModel();
                    var newTileData = liveTileVM.CreateTileData();

                    // Create the tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                    ShellTile.Create(currentSource, newTileData, true);
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Whoops!  There was a problem creating the Live Tile for this category.  Please try again later");
            }
        }

        #endregion




        void LaunchLocalSettingsPopup()
        {
            GlobalNavigationService.ToMainPageSettingsPage();
        }




        #region Page Change logic


        internal void CompletePageChangeAnimation(List<NewsItem> source, int direction = 0)
        {
            if (this.imageCache != null)
                imageCache.Flush();

            if (this.currentListBoxScroller != null)
                currentListBoxScroller.ScrollToVerticalOffset(0);

            //cl.SetNews(source, direction);
            //cl.Visibility = Visibility.Visible;

            lls.ItemsSource = source;
        }

        #endregion




        #region Navigation From, BackKeyPress

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (vm != null)
                vm.SaveTransientState();
        }

        #endregion




        ~MainPage()
        {
            DebugEx.WriteLine("MainPage {0} was finalized", header);
        }

        public void Dispose()
        {
            this.pageLevelDisposables.Dispose();

            using (this.cl)
            using (this.vm)
            using (this.adControl)
            { }

            var imageCacheHandle = this.imageCache;
            this.imageCache = null;
            System.Reactive.Concurrency.Scheduler.Default.SafelySchedule(() =>
            {
                if (imageCacheHandle != null)
                {
                    imageCacheHandle.Flush();
                    imageCacheHandle = null;
                }
            });

            Observable.Timer(TimeSpan.FromSeconds(1)).Take(1).SafelySubscribe(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }
    }
}