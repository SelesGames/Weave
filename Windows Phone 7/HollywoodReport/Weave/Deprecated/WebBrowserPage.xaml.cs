using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;

namespace weave
{
    public partial class WebBrowserPage : WeavePage, IDisposable
    {
        bool suppressNext = false;
        WebBrowserPageViewModel viewModel;
        CompositeDisposable disposables = new CompositeDisposable();

        ApplicationBarIconButton backButton, forwardButton;


        public WebBrowserPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.AppName.ToUpper();

            browser.IsScriptEnabled = true;

            this.backButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            this.forwardButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;

            browser.Opacity = .5;

            if (this.IsInDesignMode())
                return;
        }

        protected override void OnPageLoad(WeaveNavigationEventArgs navigationEventArgs)
        {
            base.OnPageLoad(navigationEventArgs);

            var startupTask = ServiceResolver.Get<IStartupTask>();
            startupTask.StartupComplete.Take(1).Subscribe(CompletePageLoad);
        }

        void CompletePageLoad()
        {
            viewModel = AppSettings.TombstoneState.ActiveWebBrowserPageViewModel;// ViewModelManager.Get<WebBrowserPageViewModel>();
            if (viewModel == null || viewModel.NewsItem == null)
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                return;
            }

            //Binding b = new Binding("IsInstapaperMobilizerEnabled") { Source = viewModel };
            //mobilizerToggle.SetBinding(ToggleSwitch.IsCheckedProperty, b);

            InitializeBrowser();

            Observable.FromEventPattern<OrientationChangedEventArgs>(GlobalNavigationService.CurrentFrame, "OrientationChanged")
                .Select(o => o.EventArgs)
                .Subscribe(e =>
                {
                    if (e.Orientation.IsAnyLandscape())
                        TitlePanel.Visibility = Visibility.Collapsed;
                    else if (e.Orientation.IsAnyPortrait())
                        TitlePanel.Visibility = Visibility.Visible;
                })
                .DisposeWith(disposables);

            mobilizerToggle.IsChecked = viewModel.IsInstapaperMobilizerEnabled;

            //suppressNext = true;
            //browser.Source = viewModel.CurrentUri.ToUri();
            //ReevaluateButtons();

            OnNavigatedTo();

            this.NavigatedTo.Subscribe(OnNavigatedTo);
        }

        void OnNavigatedTo()
        {
            if (this.viewModel == null || this.viewModel.CurrentUri == null || viewModel.NewsItem == null)
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                return;
            }

            suppressNext = true;
            browser.Source = viewModel.CurrentUri.ToUri();
            ReevaluateButtons();
        }

        void InitializeBrowser()
        {
            browser.Navigating += (s, e) =>
            {
                DebugEx.WriteLine("Navigating to: {0}", e.Uri.OriginalString);
                progressBar.IsIndeterminate = true;
                progressBar.Visibility = System.Windows.Visibility.Visible;
                browser.Opacity = .5;
            };

            browser.Navigated += (s, e2) =>
            {
                if (AppSettings.PermanentState.IsInstapaperMobilizerEnabled)
                {
                    browser.Visibility = Visibility.Collapsed;
                    RecolorPageIfUsingMobilizer();
                    Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(1).ObserveOnDispatcher().Subscribe(notUsed =>
                        browser.Visibility = Visibility.Visible);

                    Observable.FromEventPattern<NavigationEventArgs>(browser, "LoadCompleted").Take(1).Subscribe(nu =>
                    {
                        RecolorPageIfUsingMobilizer();
                    });
                }

                progressBar.IsIndeterminate = false;
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                browser.Opacity = 1;

                if (suppressNext)
                {
                    suppressNext = false;
                    return;
                }

                viewModel.Insert(e2.Uri.OriginalString);
                ReevaluateButtons();
            };
        }

        void backButton_Click(object sender, EventArgs e)
        {
            if (viewModel.CurrentUri != null && viewModel.HasPrevious)
            {
                suppressNext = true;
                viewModel.Previous();
                browser.Source = viewModel.CurrentUri.ToUri(); ;
                ReevaluateButtons();
            }
        }

        void forwardButton_Click(object sender, EventArgs e)
        {
            if (viewModel.CurrentUri != null && viewModel.HasNext)
            {
                suppressNext = true;
                viewModel.Next();
                browser.Source = viewModel.CurrentUri.ToUri();
                ReevaluateButtons();
            }
        }

        void ReevaluateButtons()
        {
            backButton.IsEnabled = true;
            forwardButton.IsEnabled = true;

            if (viewModel.CurrentUri == null)
            {
                backButton.IsEnabled = false;
                forwardButton.IsEnabled = false;
                return;
            }

            if (!viewModel.HasPrevious)
                backButton.IsEnabled = false;

            if (!viewModel.HasNext)
                forwardButton.IsEnabled = false;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //if (adControl.IsAdCurrentlyEngaged)
            //{
            //    return;
            //}

            if (SelesGames.PopupService.IsOpen)
            {
                e.Cancel = true;
                return;
            }
            browser.Visibility = Visibility.Collapsed;
            base.OnBackKeyPress(e);
            this.Dispose();
        }

        public void Dispose()
        {
            disposables.Dispose();
            //adControl.Dispose();
        }

        void mobilizerToggle_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.IsInstapaperMobilizerEnabled = true;
            if (viewModel.CurrentUri != null)
            {
                suppressNext = true;
                browser.Source = viewModel.CurrentUri.ToUri();
                ReevaluateButtons();
            }
        }

        void mobilizerToggle_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.IsInstapaperMobilizerEnabled = false;
            if (viewModel.CurrentUri != null)
            {
                suppressNext = true;
                browser.Source = viewModel.CurrentUri.ToUri();
                ReevaluateButtons();
            }
        }

        bool themeColorsHaveBeenSet = false;
        OSTheme currentTheme = OSTheme.Dark;
        Color accentColor = new Color();

        void RecolorPageIfUsingMobilizer()
        {
            if (!themeColorsHaveBeenSet)
            {
                currentTheme = OSThemeHelper.GetCurrentTheme();
                accentColor = OSThemeHelper.GetCurrentAccentColor();
                themeColorsHaveBeenSet = true;
            }

            string foreground = null, background = null, accent = null;

            if (currentTheme == OSTheme.Light)
            {
                foreground = "black";
                background = "white";
            }
            else if (currentTheme == OSTheme.Dark)
            {
                foreground = "white";
                background = "black";
            }

            accent = string.Format(
                "#{0:X2}{1:X2}{2:X2}",
                accentColor.R,
                accentColor.G,
                accentColor.B);

            DebugEx.WriteLine("about to invoke script");
            try
            {
                browser.InvokeScript("colorFontsAndBackground", accent, foreground, background);
            }
            catch (Exception) { }
        }

        void shareButton_Click(object sender, System.EventArgs e)
        {
            var pservice = new SelesGames.PopupService<string>();

            var newAppBar = new ApplicationBar();
            var closeShareButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.share.png", UriKind.Relative), Text = "close share" };
            closeShareButton.GetClick().Take(1).Subscribe(pservice.Hide)
                .DisposeWith(this.disposables);

            newAppBar.Buttons.Add(closeShareButton);

            pservice.PopupAppBar = newAppBar;


            SocialShareContextMenuControl socialShareControl;

            //if (this.viewModel != null && this.viewModel.NewsItem != null && this.viewModel.NewsItem.IsNew)
            //    socialShareControl = ServiceResolver.Get<SocialShareContextMenuControl>("complementary");
            //else
            //    socialShareControl = ServiceResolver.Get<SocialShareContextMenuControl>("accent");

            socialShareControl = ServiceResolver.Get<SocialShareContextMenuControl>("chrome");

            socialShareControl.HideCloseButtonForAppBarSetup();

            pservice.Show(socialShareControl).Take(1).Select(o => o.Result).Subscribe(OnShareMenuItemClick)
                .DisposeWith(this.disposables);
        }

        void OnShareMenuItemClick(string menuItem)
        {
            if (this.viewModel == null || this.viewModel.NewsItem == null || menuItem == null)
                return;

            var newsItem = this.viewModel.NewsItem;

            if (menuItem == "email")
                newsItem.ShareToEmail();
            else if (menuItem == "facebook")
                newsItem.ShareToSocial();
            else if (menuItem == "twitter")
                newsItem.ShareToSms();
            else if (menuItem == "instapaper")
                newsItem.SendToInstapaper();
            else if (menuItem == "ie")
                newsItem.SendToInternetExplorer();
        }
    }
}