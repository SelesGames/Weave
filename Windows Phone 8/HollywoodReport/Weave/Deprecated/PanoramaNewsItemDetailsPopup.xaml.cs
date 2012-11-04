using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SelesGames;

namespace weave
{
    public partial class PanoramaNewsItemDetailsPopup : UserControl
    {
        public event EventHandler HideStarted;
        NewsItem lastNewsItem;
        CompositeDisposable disposables = new CompositeDisposable();

        public PanoramaNewsItemDetailsPopup()
        {
            InitializeComponent();
            //Opacity = 1;
            Visibility = System.Windows.Visibility.Collapsed;
            if (this.IsInDesignMode())
                return;

            IsHitTestVisible = false;

            //LayoutRoot.GetMouseLeftButtonTap().Subscribe(OnArticleTap);

            this.title.GetSuperMouseLeftButtonTap()
                .Merge(this.timestamp.GetSuperMouseLeftButtonTap())
                .Merge(this.scroller.GetSuperMouseLeftButtonTap())
                .Subscribe(OnFullArticleTap);

            var blackBrush = new SolidColorBrush(Colors.Black);
            this.appBar.complementFill.Fill = blackBrush;
            this.appBar.accentFill.Fill = blackBrush;

            var whiteBrush = new SolidColorBrush(Colors.White);
            this.appBar.ellipsesButton.Foreground = whiteBrush;
            this.appBar.viewArticleButton.Foreground = whiteBrush;
            this.appBar.shareArticleButton.Foreground = whiteBrush;
            this.appBar.markReadButton.Foreground = whiteBrush;
            this.appBar.viewArticleButton.BorderBrush = whiteBrush;
            this.appBar.shareArticleButton.BorderBrush = null;
            this.appBar.markReadButton.BorderBrush = whiteBrush;

            this.appBar.ViewArticleButtonClicked.Subscribe(OnViewArticleButtonClicked);

            this.appBar.MarkReadButtonClicked.Subscribe(OnMarkReadButtonClicked);

            this.appBar.ShareArticleButtonClicked.Subscribe(OnShareButtonClicked);
        }




        #region View full article button handling

        void OnViewArticleButtonClicked()
        {
            if (this.lastNewsItem == null)
                return;

            this.lastNewsItem.HasBeenViewed = true;
            GlobalNavigationService.ToWebBrowserPage(this.lastNewsItem);
            Hide();
        }

        #endregion




        #region Share button handling

        void OnShareButtonClicked()
        {
            ShowShareMenu();
        }

        void ShowShareMenu()
        {
            var pservice = new SelesGames.PopupService<string>();

            SocialShareContextMenuControl socialShareControl;

            socialShareControl = ServiceResolver.Get<SocialShareContextMenuControl>("accent");

            pservice.Show(socialShareControl).Take(1).Select(o => o.Result).Subscribe(OnShareMenuItemClick)
                .DisposeWith(this.disposables);
        }

        void OnShareMenuItemClick(string menuItem)
        {
            if (this.lastNewsItem == null || menuItem == null)
                return;

            if (menuItem == "email")
                this.lastNewsItem.ShareToEmail();
            else if (menuItem == "social")
                this.lastNewsItem.ShareToSocial();
            else if (menuItem == "sms")
                this.lastNewsItem.ShareToSms();
            else if (menuItem == "instapaper")
                this.lastNewsItem.SendToInstapaper();
            else if (menuItem == "ie")
                this.lastNewsItem.SendToInternetExplorer();
        }

        #endregion




        #region Mark article as read button handling

        void OnMarkReadButtonClicked()
        {
            MarkCurrentArticleRead();
        }

        void MarkCurrentArticleRead()
        {
            if (this.lastNewsItem != null)
                this.lastNewsItem.HasBeenViewed = true;
            Hide();
            PanoramaPageDataStream.CheckForNewNewsItemsAndPushToStream();
        }

        #endregion




        void OnFullArticleTap()
        {
            if (this.appBar.IsExtended)
            {
                this.appBar.CloseExtended(true);
            }
            else
            {
                Hide();
            }
        }

        public void Show(NewsItem newsItem)
        {
            disposables.Clear();

            OGCloseSB.Stop();
            ContentFadeInSB.Stop();
            //FadeInSidebarsSB.Stop();

            IsHitTestVisible = true;
            bool dontSetNewsItem = false;
            if (lastNewsItem != newsItem)
                scroller.ScrollToVerticalOffset(0);
            else
                dontSetNewsItem = true;
            lastNewsItem = newsItem;

            title.Text = newsItem.Title;
            timestamp.Text = newsItem.FormattedForPopupsSourceAndDate;

            descriptionContainer.Opacity = 0;
            Visibility = System.Windows.Visibility.Visible;


            if (!dontSetNewsItem)
                OGPopupSB
                    .BeginWithNotification()
                    .Subscribe(notused => SetArticleDescription())
                    .DisposeWith(disposables);

            else
                OGPopupSB
                   .BeginWithNotification()
                   .Subscribe(notused => ContentFadeInSB.Begin())
                   .DisposeWith(disposables);
        }

        void SetArticleDescription()
        {
            descriptionContainer.Children.Clear();
            image.DataContext = lastNewsItem;

            descriptionContainer.Children.Add(image);

            Style dtbs = Resources["dtbs"] as Style;

            var paragraphs = new[] { "temp" };// RssServiceLayer.Sanitize(lastNewsItem.Description);

            Observable.Interval(TimeSpan.FromMilliseconds(1))
                .Zip(paragraphs, (l, paragraph) => paragraph)
                .ObserveOnDispatcher()
                .Subscribe(
                    o => descriptionContainer.Children.Add(new TextBlock { Text = o, Style = dtbs }),
                    exception => { ; },
                    () => descriptionContainer.Children.Add(filler));

            //if (AdVisibilityService.AreAdsStillBeingShownAtAll)
            //    descriptionContainer.Children.Add(adControl);

            ContentFadeInSB.Begin();
        }

        public void Hide()
        {
            disposables.Clear();
            IsHitTestVisible = false;
            HideStarted.Raise(this);

            var disp = OGCloseSB.BeginWithNotification().Subscribe(notUsed =>            
            {
                //OGCloseSB.Stop();
                Visibility = System.Windows.Visibility.Collapsed;
            });
            disposables.Add(disp);
            //FadeOutSidebarsSB.Begin();
        }
    }
}
