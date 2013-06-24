using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using SelesGames.UI.Advertising;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Weave.Customizability;
using Weave.ViewModels;


namespace weave
{
    public partial class MainPage : PhoneApplicationPage, IDisposable
    {
        bool isPageInitialized = false;
        string header = null;
        string mode = null;
        Guid? feedId = null;

        PermanentState permState;

        MainPageViewModel vm;
        MainPageSourceListViewModel vmSourcesList;

        SwitchingAdControl adControl;

        ApplicationBarIconButton refreshButton, fontButton, markPageReadButton, addSourceButton, searchSourceButton;
        ApplicationBarMenuItem lockOrientationButton, openNavMenuButton, pinToStartScreenButton; 
        CompositeDisposable pageLevelDisposables = new CompositeDisposable();
        SwipeGestureHelper swipeHelper;
        MenuMode currentMenuMode = MenuMode.Hidden;
        IApplicationBar mainAppBar, sourcesListAppBar;

        SelesGames.PopupService<Unit> fontSizePopupService;
        FontAndThemePopup fontSizePopup;




        #region Constructor

        public MainPage()
        {
            InitializeComponent();
            SourcesList.Visibility = Visibility.Visible;
            MinTitlePanel.RenderTransform = new CompositeTransform();
            ContentGrid.RenderTransform = new CompositeTransform();
            cl.RenderTransform = new CompositeTransform();
            SourcesList.RenderTransform = new CompositeTransform();

            if (DesignerProperties.IsInDesignTool)
                return;

            this.Loaded += OnLoaded;
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition());

            permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();
            var isAppBarMinimized = permState.IsHideAppBarOnArticleListPageEnabled;
            ApplicationBar.Mode = isAppBarMinimized ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;
            bottomBarFill.Height = isAppBarMinimized ? 30d : 72d;

            CreateMainAppBar();
            CreateSourcesListAppBar();
            BindIsOrientationLockedToAppBar();

            if (OSThemeHelper.GetCurrentTheme() == OSTheme.Light)
                fade.Fill = Resources["LightThemeFade"] as System.Windows.Media.Brush;

            fontSizePopup = ServiceResolver.Get<FontAndThemePopup>();
            Observable.FromEventPattern<SelesGames.EventArgs<ArticleListFormatProperties>>(fontSizePopup, "ArticleListFormatChanged")
                .Subscribe(o => OnArticleListFormatChanged(fontSizePopup, o.EventArgs)).DisposeWith(pageLevelDisposables);
        }

        void OnArticleListFormatChanged(object sender, SelesGames.EventArgs<ArticleListFormatProperties> eventArgs)
        {
            cl.ArticleTheme = permState.ArticleListFormat;
        }

        void CreateMainAppBar()
        {
            mainAppBar = ApplicationBar;
            markPageReadButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            refreshButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            fontButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            lockOrientationButton = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            pinToStartScreenButton = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            openNavMenuButton = ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;
        }

        void CreateSourcesListAppBar()
        {
            sourcesListAppBar = new ApplicationBar
            {
                BackgroundColor = mainAppBar.BackgroundColor,
                ForegroundColor = mainAppBar.ForegroundColor,
                Mode = ApplicationBarMode.Default,
                Opacity = mainAppBar.Opacity,
                IsMenuEnabled = mainAppBar.IsMenuEnabled,
                IsVisible = mainAppBar.IsVisible,
            };

            searchSourceButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.feature.search.rest.png", UriKind.Relative)) { Text = "search" };
            addSourceButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.add.rest.png", UriKind.Relative)) { Text = "add" };

            searchSourceButton.GetClick().Subscribe(o => OnSearchSourceClick()).DisposeWith(this.pageLevelDisposables);
            addSourceButton.GetClick().Subscribe(o => OnAddSourceButtonClick()).DisposeWith(this.pageLevelDisposables);

            sourcesListAppBar.Buttons.Add(searchSourceButton);
            sourcesListAppBar.Buttons.Add(addSourceButton);
        }

        void OnSearchSourceClick()
        {
            NavigationService.ToAddSourcePage();
        }
        void OnAddSourceButtonClick()
        {
            NavigationService.ToAddSourcePage();
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
            }
            else if (Orientation == PageOrientation.LandscapeLeft)
            {
                rightBarFill.Visibility = Visibility.Visible;
                leftBarFill.Visibility = Visibility.Collapsed;
                bottomBarFill.Visibility = Visibility.Collapsed;
            }
            else if (Orientation == PageOrientation.LandscapeRight)
            {
                leftBarFill.Visibility = Visibility.Visible;
                rightBarFill.Visibility = Visibility.Collapsed;
                bottomBarFill.Visibility = Visibility.Collapsed;
            }
        }

        void BindIsOrientationLockedToAppBar()
        {
            var orientationLockService = ServiceResolver.Get<OrientationLockService>();

            var bindingAdapter = new ApplicationBarToggleMenuItemAdapter(lockOrientationButton)
            {
                CheckedText = "turn off \"soft\" rotation lock",
                UncheckedText = "turn on \"soft\" rotation lock",
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
                        await this.GetLoaded().Take(1).ToTask();
                        await vm.OnNavigatedTo(e.NavigationMode);
                    }
                }
                else
                {
                    isPageInitialized = true;

                    if (NavigationContext.QueryString.ContainsKey("header"))
                    {
                        header = NavigationContext.QueryString["header"];
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

                    if (mode == "sources")
                    {
                        ShowMenuNoAnimation();
                    }
                    else
                    {
                        HideMenuNoAnimation();
                    }


                    //CreateViewModel();
                    //await Task.Yield();

                    FinishPageInitialization();

                    CreateViewModel();
                    vmSourcesList = new MainPageSourceListViewModel();
                    SourcesList.DataContext = vmSourcesList;

                    //await TimeSpan.FromSeconds(0.4);

                    if (vm != null)
                        await vm.InitializeAsync();

                    vmSourcesList.RefreshCategories();
                }
            }
            catch (Exception exception)
            {
                DebugEx.WriteLine("exception in MainPage onnavto, {0}", exception);
                NavigationService.TryGoBack();
            }
        }

        void CreateViewModel()
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                DataContext = null;
                this.vm = null;
                return;
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
        }




        #region DELAYED PAGE INTIALIZATION - performed on initial page navigation.  This delayed form of initialization allows for faster intial page draw when navigating to this page

        void FinishPageInitialization()
        {
            InitializeAdControl();
            InitializeCustomListAndImageCache();
            InitializeTouchSwipingHandling();
            InitializeApplicationBarButtonEventHandlers();
            InitializeNavMenuHandlers();
            pinToStartScreenButton.IsEnabled = IsPinToStartButtonEnabled();
        }

        async void InitializeAdControl()
        {
            if (!AdVisibilityService.AreAdsStillBeingShownAtAll)
                return;

            adControl = new SwitchingAdControl(ServiceResolver.Get<AdControlFactory>(), this.header);
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
        
        void InitializeCustomListAndImageCache()
        {
            cl.ArticleTheme = permState.ArticleListFormat;
            SubscribeToNewsItemClicked();
        }

        void InitializeTouchSwipingHandling()
        {
            swipeHelper = new SwipeGestureHelper(LayoutRoot);
            swipeHelper.Swipe += OnSwipe;
            swipeHelper.DisposeWith(pageLevelDisposables);
        }

        void OnSwipe(object sender, SwipeGestureHelper.SwipeEventArgs e)
        {
            if (e.Direction == SwipeGestureHelper.SwipeDirection.Left)
                OnPreviousPage();
            else if (e.Direction == SwipeGestureHelper.SwipeDirection.Right)
                OnNextPage();
        }

        /// <summary>
        /// Initialize all button handlers on page (refresh, next/previous page, font, mark page read)
        /// </summary>
        void InitializeApplicationBarButtonEventHandlers()
        {
            refreshButton.GetClick().Where(_ => vm != null).Subscribe(() => vm.ManualRefresh()).DisposeWith(pageLevelDisposables);
            fontButton.GetClick().Subscribe(LaunchLocalSettingsPopup).DisposeWith(pageLevelDisposables);
            markPageReadButton.GetClick().Subscribe(OnAllRead).DisposeWith(pageLevelDisposables);
            pinToStartScreenButton.GetClick().Subscribe(OnPinToStartButtonPressed).DisposeWith(pageLevelDisposables);
            openNavMenuButton.GetClick().Subscribe(ShowMenu).DisposeWith(pageLevelDisposables);
        }

        void InitializeNavMenuHandlers()
        {
            SourcesList.ItemSelected += SourcesList_ItemSelected;
        }

        void SourcesList_ItemSelected(object sender, CategoryOrFeedEventArgs e)
        {
            var catVM = e.Selected;
            if (catVM == null)
                return;

            string header = null;
            string mode = null;
            Guid? feedId = null;

            header = catVM.Name;

            if (catVM.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category)
            {
                mode = "category";
            }
            else if (catVM.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed)
            {
                mode = "feed";
                feedId = catVM.FeedId;
            }

            if (mode == this.mode && header == this.header)
                HideMenu();
            else
            {
                this.header = header;
                this.mode = mode;
                this.feedId = feedId;

                CreateViewModel();
                vm.InitializeAsync().ContinueWith(_ => Dispatcher.BeginInvoke(() => vmSourcesList.RefreshCategories()));
                HideMenu();
            }
        }

        #endregion




        #region DropDown menu event handling

        void ShowMenuNoAnimation()
        {
            currentMenuMode = MenuMode.Displayed;
            MinTitlePanel.SoftCollapse();
            ContentGrid.SoftCollapse();
            SourcesList.SoftMakeVisible();
            ApplicationBar = sourcesListAppBar;
        }

        void ShowMenu()
        {
            if (currentMenuMode == MenuMode.Displayed)
                return;

            currentMenuMode = MenuMode.Displayed;
            MinTitlePanel.IsHitTestVisible = false;
            ContentGrid.IsHitTestVisible = false;
            SourcesList.SoftMakeVisible();
            HideCategoriesListSB.Stop();
            ShowCategoriesListSB.Begin();
            ApplicationBar = sourcesListAppBar;
        }

        void HideMenuNoAnimation()
        {
            currentMenuMode = MenuMode.Hidden;
            SourcesList.SoftCollapse();;
            MinTitlePanel.SoftMakeVisible();
            ContentGrid.SoftMakeVisible();
            ApplicationBar = mainAppBar;
        }

        void HideMenu()
        {
            if (currentMenuMode == MenuMode.Hidden)
                return;

            currentMenuMode = MenuMode.Hidden;
            SourcesList.IsHitTestVisible = false;
            MinTitlePanel.IsHitTestVisible = true;
            ContentGrid.IsHitTestVisible = true;
            ShowCategoriesListSB.Stop();
            HideCategoriesListSB.Begin();
            ApplicationBar = mainAppBar;
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
            catch (InvalidOperationException ex)
            {
                DebugEx.WriteLine(ex);
                if (ex.Message == "image count")
                    MessageBox.Show("We can only make a Live Tile out of a news source that has at least 2 images in it!");
                else
                    MessageBox.Show("Whoops!  There was a problem creating the Live Tile for this category.  Please try again later");
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
            //GlobalNavigationService.ToMainPageSettingsPage();
            if (PopupService.IsOpen)
                return;

            fontSizePopupService = new SelesGames.PopupService<System.Reactive.Unit>(fontSizePopup)
            {
                CloseOnNavigation = false,
            };
            fontSizePopupService.BeginShow();
            //Observable.FromEventPattern<EventArgs<FontSizeProperties>>(fontSizePopup, "FontSizeChanged")
            //    .Subscribe(o => OnFontSizeChanged(fontSizePopup, o.EventArgs)).DisposeWith(disposables);
            //Observable.FromEventPattern<EventArgs<FontProperties>>(fontSizePopup, "FontChanged")
            //    .Subscribe(o => OnFontChanged(fontSizePopup, o.EventArgs)).DisposeWith(disposables);
        }




        #region Page Change logic

        void OnPreviousPage()
        {
            if (vm != null && vm.HasPrevious)
            {
                PreparePageChangeAnimation(PageChangeAnimateDirection.Previous);
                vm.CurrentPage--;
            }
            else if (currentMenuMode == MenuMode.Hidden)
            {
                ShowMenu();
            }
        }

        void OnNextPage()
        {
            if (vm != null && currentMenuMode == MenuMode.Displayed)
            {
                HideMenu();
            }
            else if (vm != null && vm.HasNext)
            {
                PreparePageChangeAnimation(PageChangeAnimateDirection.Next);
                vm.CurrentPage++;
            }
        }

        enum PageChangeAnimateDirection
        {
            Previous,
            Next
        }

        enum MenuMode
        {
            Displayed,
            Hidden
        }




        #region Page Change animation helpers

        bool isInPageChangeAnimation = false;

        void PreparePageChangeAnimation(PageChangeAnimateDirection direction)
        {
            if (isInPageChangeAnimation)
                return;

            isInPageChangeAnimation = true;

            if (direction == PageChangeAnimateDirection.Previous)
                previousPageStartSB.Begin();

            else if (direction == PageChangeAnimateDirection.Next)
                nextPageStartSB.Begin();
        }

        internal void CompletePageChangeAnimation(List<NewsItem> source, int direction = 0)
        {
            isInPageChangeAnimation = false;
            this.nextPageStartSB.Stop();
            this.previousPageStartSB.Stop();

            cl.AnimationDirection = direction;
            cl.ItemsSource = source;
            cl.Visibility = Visibility.Visible;
        }

        #endregion




        #endregion




        #region Navigation From, BackKeyPress

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (vm != null)
                vm.SaveTransientState();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (PopupService.IsOpen)
                return;

            IsHitTestVisible = false;
            base.OnBackKeyPress(e);
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

            if (swipeHelper != null)
                swipeHelper.Swipe -= OnSwipe;

            Observable.Timer(TimeSpan.FromSeconds(1)).Take(1).SafelySubscribe(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }
    }
}