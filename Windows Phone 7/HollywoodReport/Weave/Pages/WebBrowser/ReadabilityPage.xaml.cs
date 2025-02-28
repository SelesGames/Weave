﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using Telerik.Windows.Controls;

namespace weave
{
    public partial class ReadabilityPage : PhoneApplicationPage, IDisposable
    {
        ReadabilityPageViewModel viewModel;
        bool isHtmlDisplayed = false;
        bool isArticleNonDisplayable = false;
        CompositeDisposable disposables = new CompositeDisposable();
        SerialDisposable setArticleHandle = new SerialDisposable();
        Brush opacityMask;
        SelesGames.PopupService<string> sharePopupService;
        SelesGames.PopupService<Unit> fontSizePopupService;
        FontSizePopup fontSizePopup;
        SocialShareContextMenuControl socialSharePopup;

        public ReadabilityPage()
        {
            InitializeComponent();

            browser.Opacity = 0d;
            browser.IsScriptEnabled = true;
            opacityMask = browser.OpacityMask;
            HideLoadingIndicators();
            fill.SetBinding(Rectangle.FillProperty, new Binding("CurrentTheme.BackgroundBrush") { Source = AppSettings.Instance.Themes });
            this.Loaded += OnLoaded;
            InitializeFlickHandling();

            fontSizePopup = ServiceResolver.Get<FontSizePopup>();
            socialSharePopup = ServiceResolver.Get<SocialShareContextMenuControl>("accent");

            var permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();
            var isAppBarMinimized = permState.IsHideAppBarOnArticleViewerPageEnabled;
            ApplicationBar.Mode = isAppBarMinimized ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;
            bottomBarFill.Height = isAppBarMinimized ? 30d : 72d;
        }




        #region Event Handlers

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged += OnOrientationChanged;
            this.Unloaded += OnUnloaded;
            ChangeBrowserMarginsByPageLayout();
            AppSettings.Instance.Themes.PropertyChanged += OnThemeChanged;
        }

        void BindIsFavoriteToAppBar()
        {
            var favoriteButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            var bindingAdapter = new ApplicationBarToggleIconButtonAdapter(favoriteButton)
            {
                CheckedText = "unfavorite",
                UncheckedText = "favorite",
                CheckedIconUri = new Uri("/Assets/Icons/appbar.heart.png", UriKind.Relative),
                UncheckedIconUri = new Uri("/Assets/Icons/appbar.heart.outline.png", UriKind.Relative),
                IsChecked = viewModel.NewsItem.IsFavorite,
            };
            bindingAdapter.SetBinding(ApplicationBarToggleIconButtonAdapter.IsCheckedProperty,
                 new Binding("IsFavorite") { Source = viewModel.NewsItem, Mode = BindingMode.TwoWay });
            bindingAdapter.DisposeWith(disposables);
        }

        void BindIsOrientationLockedToAppBar()
        {
            var orientationLockButton = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            var orientationLockService = ServiceResolver.Get<OrientationLockService>();

            var bindingAdapter = new ApplicationBarToggleMenuItemAdapter(orientationLockButton) 
            {
                CheckedText = "turn off phone tilt helper",
                UncheckedText = "turn on phone tilt helper",
                IsChecked = orientationLockService.IsLocked,
            };
            bindingAdapter.SetBinding(ApplicationBarToggleMenuItemAdapter.IsCheckedProperty,
                 new Binding("IsLocked") { Source = orientationLockService, Mode = BindingMode.TwoWay });
            bindingAdapter.DisposeWith(disposables);
        }

        void BindIsMarkedReadToAppBar()
        {
            if (viewModel == null || viewModel.NewsItem == null)
                return;

            var markedReadButton = ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;

            var bindingAdapter = new ApplicationBarToggleMenuItemAdapter(markedReadButton)
            {
                CheckedText = "mark unread",
                UncheckedText = "mark read",
                IsChecked = viewModel.NewsItem.HasBeenViewed,
            };
            bindingAdapter.SetBinding(ApplicationBarToggleMenuItemAdapter.IsCheckedProperty,
                 new Binding("HasBeenViewed") { Source = viewModel.NewsItem, Mode = BindingMode.TwoWay });
            bindingAdapter.DisposeWith(disposables);
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

        // Remove event handlers on page unloaded
        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged -= OnOrientationChanged;
            this.Unloaded -= OnUnloaded;
            AppSettings.Instance.Themes.PropertyChanged -= OnThemeChanged;
        }

        void OnThemeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentTheme")
                return;

            var theme = AppSettings.Instance.Themes.CurrentTheme;
            ColorToTheme(theme);
        }

        void ColorToTheme(Theme theme)
        {
            try
            {
                browser.InvokeScript("colorFontAndBackground", theme.Text, theme.Background);
            }
            catch (Exception) { }
        }

        #endregion




        #region Initialize Touch Swiping/Flicking handling

        void InitializeFlickHandling()
        {
            var gestureListener = GestureService.GetGestureListener(LayoutRoot);

            Observable.FromEventPattern<FlickGestureEventArgs>(
                e => gestureListener.Flick += e,
                e => gestureListener.Flick -= e)
                .Where(o => o.EventArgs.Direction == System.Windows.Controls.Orientation.Horizontal)
                .Select(o => o.EventArgs.HorizontalVelocity)
                .SafelySubscribe(OnHorizontalSwipe)
                .DisposeWith(disposables);
        }

        void OnHorizontalSwipe(double velocity)
        {
            if (velocity > 700)
                OnLeftSwipe();
            else if (velocity < -700)
                OnRightSwipe();
        }

        void OnLeftSwipe()
        {
            IsHitTestVisible = false;
            SetValue(RadTransitionControl.TransitionProperty, 
                new RadSlideTransition 
                { 
                    PlayMode = TransitionPlayMode.Simultaneously, 
                    Orientation = System.Windows.Controls.Orientation.Horizontal 
                });
            NavigationService.SafelyGoBackIfPossible();
        }

        void OnRightSwipe()
        {
            ShowSharingPopup("horizontal");
        }

        #endregion


 

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.NavigationMode == NavigationMode.New)
                    LoadingInSB.Begin();

                // Check to see if the article was not displayable due to an Exception.  If so, go back to previous page.
                if (isArticleNonDisplayable)
                {
                    NavigationService.SafelyGoBackIfPossible();
                    return;
                }

                if (viewModel == null)
                {
                    var isViewModelInit = await InitializeViewModelAsync();
                    if (!isViewModelInit)
                    {
                        NavigationService.SafelyGoBackIfPossible();
                        return;
                    }
                    else
                        CompleteInitialization();
                }

                if (!isHtmlDisplayed || UserSwitchedViewingType)
                {
                    await DisplayArticleContent();
                    SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition { PlayMode = TransitionPlayMode.Simultaneously });
                }
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                NavigationService.SafelyGoBackIfPossible();
            }
        }

        void CompleteInitialization()
        {
            BindIsFavoriteToAppBar();
            BindIsOrientationLockedToAppBar();
            BindIsMarkedReadToAppBar();
        }

        bool UserSwitchedViewingType
        {
            get
            {
                return viewModel.NewsItem.FeedSource != null && viewModel.LastViewingType != viewModel.NewsItem.FeedSource.ArticleViewingType;
            }
        }

        async Task<bool> InitializeViewModelAsync()
        {
            var ts = await AppSettings.Instance.TombstoneState.Get();
            var vm = ts.ActiveWebBrowserPageViewModel;
            if (vm == null || vm.NewsItem == null)
                return false;

            if (vm.NewsItem.FeedSource == null)
            {
                var feeds = await ServiceResolver.Get<weave.Data.Weave4DataAccessLayer>().Feeds.Get();
                vm.NewsItem.FeedSource = feeds.Single(o => o.Id.Equals(vm.NewsItem.FeedId));
            }

            viewModel = new ReadabilityPageViewModel { NewsItem = vm.NewsItem };
            return true;
        }




        async Task DisplayArticleContent()
        {
            //IsHitTestVisible = false;
            browser.Opacity = 0d;
            browser.OpacityMask = opacityMask;
            ShowLoadingIndicators();
            setArticleHandle.Disposable = null;

            try
            {
                if (viewModel != null &&
                    viewModel.NewsItem != null &&
                    viewModel.NewsItem.FeedSource != null &&
                    (viewModel.NewsItem.FeedSource.ArticleViewingType == ArticleViewingType.InternetExplorer || viewModel.NewsItem.FeedSource.ArticleViewingType == ArticleViewingType.InternetExplorerOnly))
                {
                    viewModel.LastViewingType = ArticleViewingType.InternetExplorer;
                    await browser.NavigateAsync(new Uri(viewModel.NewsItem.Link, UriKind.Absolute), null, "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0");//User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; XBLWP7; ZuneWP7)
                                                                                                                                                                                                         //User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; NOKIA; Lumia 900)
                }
                else
                {
                    var html = await viewModel.GetMobilizedArticleHtml();
                    await browser.NavigateToStringAsync(html);
                }

                isHtmlDisplayed = true;
                browser.OpacityMask = null;

                await LoadingOutSB.BeginWithNotification().ToTask();
                setArticleHandle.Disposable = browser.GetNavigating().Subscribe(OnBrowserNavigating);
                //IsHitTestVisible = true;
                HideLoadingIndicators();
            }
            catch (Exception ex)
            {
                isArticleNonDisplayable = true;
                DebugEx.WriteLine(ex);
                SelesGames.Phone.TaskService.ToInternetExplorerTask(viewModel.NewsItem.Link);
                try
                {
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }catch { }
            }
        }

        void ShowLoadingIndicators()
        {
            new FrameworkElement[] { fill, BusyIndicator }.ToList().ForEach(o => o.Visibility = Visibility.Visible);
            BusyIndicator.Opacity = 1d;
            BusyIndicator.IsPlaying = true;
        }

        void HideLoadingIndicators()
        {
            new FrameworkElement[] { fill, BusyIndicator }.ToList().ForEach(o => o.Visibility = Visibility.Collapsed);
            BusyIndicator.IsPlaying = false;
        }

        //void browser_Navigated(object sender, NavigationEventArgs e)
        //{
        //    browser.Navigating += browser_Navigating;
        //    //await TaskEx.Delay(100);
        //    //browser.Opacity = 1d;
        //}

        void OnBrowserNavigating(NavigatingEventArgs e)
        {
            var uri = e.Uri;
            if (uri == null)
                return;

            // special youtube rule
            var originalString = uri.OriginalString;
            if (originalString != null && originalString.StartsWith("http://m.youtube.com/#/watch"))
                return;

            e.Cancel = true;
            SelesGames.Phone.TaskService.ToInternetExplorerTask(uri);
        }




        #region Button and Menu Item handling

        void favoriteButton_Click(object sender, EventArgs e)
        {
            viewModel.NewsItem.IsFavorite = true;
        }






        #region Font / Font Size change handlers

        void fontButton_Click(object sender, EventArgs e)
        {
            if (PopupService.IsOpen)
                return;

            fontSizePopupService = new SelesGames.PopupService<System.Reactive.Unit>(fontSizePopup);
            fontSizePopupService.BeginShow();
            Observable.FromEventPattern<EventArgs<FontSizeProperties>>(fontSizePopup, "FontSizeChanged")
                .Subscribe(o => OnFontSizeChanged(fontSizePopup, o.EventArgs)).DisposeWith(disposables);
            Observable.FromEventPattern<EventArgs<FontProperties>>(fontSizePopup, "FontChanged")
                .Subscribe(o => OnFontChanged(fontSizePopup, o.EventArgs)).DisposeWith(disposables);
        }

        void OnFontSizeChanged(object sender, EventArgs<FontSizeProperties> e)
        {
            SetFontSize(e.Item.HtmlTextSize());
        }

        void SetFontSize(string fontSize)
        {
            try
            {
                browser.InvokeScript("setTextSize", fontSize);
            }
            catch (Exception) { }
        }

        void OnFontChanged(object sender, EventArgs<FontProperties> e)
        {
            SetFont(e.Item.FontName);
        }

        void SetFont(string fontName)
        {
            try
            {
                browser.InvokeScript("setFont", fontName);
            }
            catch (Exception) { }
        }

        #endregion




        #region Share Menu and Button Handling

        void shareButton_Click(object sender, EventArgs e)
        {
            ShowSharingPopup("vertical");
        }

        void ShowSharingPopup(string mode)
        {
            if (PopupService.IsOpen)
                return;

            if (mode == "horizontal")
                socialSharePopup.SetHorizontalMode();
            else if (mode == "vertical")
                socialSharePopup.SetVerticalMode();

            sharePopupService = new SelesGames.PopupService<string>(socialSharePopup);
            sharePopupService.BeginShow();
            Observable.FromEventPattern<EventArgs<PopupResult<string>>>(socialSharePopup, "ResultCompleted").Take(1)
                .Subscribe(o => OnSocialSharePopupResultCompleted(socialSharePopup, o.EventArgs)).DisposeWith(disposables);
        }

        void OnSocialSharePopupResultCompleted(object sender, EventArgs<PopupResult<string>> e)
        {
            var result = e.Item.Result;
            OnShareMenuItemClick(result);
        }

        void OnShareMenuItemClick(string menuItem)
        {
            if (viewModel == null || viewModel.NewsItem == null || menuItem == null)
                return;

            var newsItem = viewModel.NewsItem;

            if (menuItem == "email")
                newsItem.ShareToEmail();
            else if (menuItem == "socialShare")
                newsItem.ShareToSocial();
            else if (menuItem == "sms")
                newsItem.ShareToSms();
            else if (menuItem == "instapaper")
                newsItem.SendToInstapaper();
            else if (menuItem == "ie")
                newsItem.SendToInternetExplorer();
        }

        #endregion




        void EditSourceAppMenuItemClick(object sender, System.EventArgs e)
        {
            NavigationService.ToEditSourcePage(viewModel.NewsItem.FeedId.ToString());
        }

        #endregion




        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            IsHitTestVisible = false;
            base.OnBackKeyPress(e);
        }

        public void Dispose()
        {
            setArticleHandle.Dispose();
            disposables.Dispose();
            this.Loaded -= OnLoaded;
        }
    }
}




#region unused saved to isostorage example

        //private void SaveToIsoStore(string fileName, byte[] data)
        //{
        //    string strBaseDir = string.Empty;
        //    string delimStr = "/";
        //    char[] delimiter = delimStr.ToCharArray();
        //    string[] dirsPath = fileName.Split(delimiter);

        //    //Get the IsoStore.
        //    IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

        //    //Re-create the directory structure.
        //    for (int i = 0; i < dirsPath.Length - 1; i++)
        //    {
        //        strBaseDir = System.IO.Path.Combine(strBaseDir, dirsPath[i]);
        //        isoStore.CreateDirectory(strBaseDir);
        //    }

        //    //Remove the existing file.
        //    if (isoStore.FileExists(fileName))
        //    {
        //        isoStore.DeleteFile(fileName);
        //    }

        //    //Write the file.
        //    using (BinaryWriter bw = new BinaryWriter(isoStore.CreateFile(fileName)))
        //    {
        //        bw.Write(data);
        //        bw.Close();
        //    }
        //}

        //private void SaveToIsoStore(string html)
        //{
        //    var bytes = new UTF8Encoding(false, false).GetBytes(html);
        //    SaveToIsoStore("temp.html", bytes);
        //}
#endregion