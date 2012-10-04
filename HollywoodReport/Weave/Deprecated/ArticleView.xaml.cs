using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using weave.UI.Advertising;
using System.Windows.Media.Imaging;
using SelesGames;

namespace weave
{
    public partial class ArticleView : UserControl, IDisposable
    {
        ISubject<Unit> hideStarted = new Subject<Unit>();
        ISubject<Unit> hideCompleted = new Subject<Unit>();

        ISubject<Unit> requestPreviousPage = new Subject<Unit>();
        ISubject<Unit> requestNextPage = new Subject<Unit>();

        public IObservable<Unit> HideStarted { get { return hideStarted; } }//.AsObservable(); } }
        public IObservable<Unit> HideCompleted { get { return hideCompleted; } }//.AsObservable(); } }

        public IObservable<Unit> RequestPreviousPage { get { return requestPreviousPage.AsObservable(); } }
        public IObservable<Unit> RequestNextPage { get { return requestNextPage.AsObservable(); } }


        NewsItem lastNewsItem;
        NewsItem currentNewsItem;
        internal TrialModeAdControl adControl;

        CompositeDisposable openArticleDisposables = new CompositeDisposable();
        CompositeDisposable closeArticleDisposables = new CompositeDisposable();
        CompositeDisposable disposables = new CompositeDisposable();
        CompositeDisposable setArticleContentTimerHandle = new CompositeDisposable();


        public bool IsCloseLocked { get; set; }

        BindableMainPageFontStyle bindingSource;

        Style dtbs;
        DescriptionTextBlockFactory tbFactory;

        Guid id;

        public ArticleView()
        {
            id = Guid.NewGuid();
            DebugEx.WriteLine("ArticleView {0} created", id.ToString());
            
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            Visibility = Visibility.Collapsed;

            //grid.GetSuperMouseLeftButtonTap()
            //    .Merge(headerGrid.GetSuperMouseLeftButtonTap())
            //    //.Merge(copy.GetMouseLeftButtonTap())
            //    .Where(_ => !IsCloseLocked)
            //    .Subscribe(OnFullArticleTap)
            //    .DisposeWith(disposables);

            if (AdVisibilityService.AreAdsStillBeingShownAtAll)
            {
                adControl = new TrialModeAdControl();
                adControl.AdMargin = new Thickness(0, 0, 0, 24);
                adControl.AdHeight = 80;
                adControl.AdWidth = 480;
                adControl.PlayAnimations = false;
                //adControl.SetValue(Grid.RowProperty, 2);
                //articleContentGrid.Children.Add(adControl);
            }

            var currentTheme = OSThemeHelper.GetCurrentTheme();
            if (currentTheme == OSTheme.Dark)
            {
                this.toolbar.accentFill.Fill = AppSettings.Palette.AccentDullBrush;
                this.toolbar.complementFill.Fill = AppSettings.Palette.ComplementaryDullBrush;
            }
            else
            {
                this.toolbar.accentFill.Fill = AppSettings.Palette.AccentBrush;
                this.toolbar.complementFill.Fill = AppSettings.Palette.ComplementaryBrush;
            }

            //this.toolbar.ViewArticleButtonClicked.Subscribe(OnViewArticleButtonClicked)
            //    .DisposeWith(disposables);

            //this.toolbar.MarkReadButtonClicked.Subscribe(OnMarkReadButtonClicked)
            //    .DisposeWith(disposables);

            //this.toolbar.ShareArticleButtonClicked.Subscribe(OnShareButtonClicked)
            //    .DisposeWith(disposables);

            bindingSource = ServiceResolver.Get<BindableMainPageFontStyle>();

            this.title.SetBinding(TextBlock.FontSizeProperty, bindingSource.TitleSizeBinding);
            this.title.SetBinding(TextBlock.FontFamilyProperty, bindingSource.ThicknessBinding);

            this.feedName.SetBinding(TextBlock.FontSizeProperty, bindingSource.PublicationLineSizeBinding);
            //this.publishedDateOverlay.SetBinding(TextBlock.FontSizeProperty, bindingSource.PublicationLineSizeBinding);

            dtbs = Resources["dtbs"] as Style;

            tbFactory = new DescriptionTextBlockFactory(dtbs);
        }

        void ShowShareMenu()
        {
            var pservice = new SelesGames.PopupService<string>();

            SocialShareContextMenuControl socialShareControl;

            if (this.currentNewsItem.IsFavorite)
                socialShareControl = ServiceResolver.Get<SocialShareContextMenuControl>("complementary");
            else
                socialShareControl = ServiceResolver.Get<SocialShareContextMenuControl>("accent");

            pservice.Show(socialShareControl).Take(1).Select(o => o.Result).Subscribe(OnShareMenuItemClick)
                .DisposeWith(this.disposables);
        }

        void OnShareMenuItemClick(string menuItem)
        {
            if (this.currentNewsItem == null || menuItem == null)
                return;

            if (menuItem == "email")
                this.currentNewsItem.ShareToEmail();
            //else if (menuItem == "facebook")
            //    this.currentNewsItem.ShareToFacebook();
            //else if (menuItem == "twitter")
            //    this.currentNewsItem.ShareToTwitter();
            else if (menuItem == "instapaper")
                this.currentNewsItem.SendToInstapaper();
            else if (menuItem == "ie")
                this.currentNewsItem.SendToInternetExplorer();
        }

        public ImageCache ImageCache { get; set; }

        public void Show(NewsItem newsItem, double verticalOffset)
        {
            if (newsItem == null)
                return;

            this.lastNewsItem = this.currentNewsItem;
            this.currentNewsItem = newsItem;

            IsCloseLocked = true;

            this.closeArticleDisposables.Clear();
            DropOutSB.Pause();

            HeaderSlideToTopSB.Stop();
            HeaderFadeOutSideBarAndImageSB.Stop();
            HeaderTextWidthTransformationSB.Stop();

            this.toolbar.Visibility = Visibility.Collapsed;

            if (AdVisibilityService.AreAdsStillBeingShownAtAll)
                adControl.HideAdControl();


            if (this.toolbar.IsExtended)
                this.toolbar.CloseExtended();


            SetArticleHeader();

            bool dontSetNewsItem = false;
            if (this.lastNewsItem != this.currentNewsItem)
            {
                this.setArticleContentTimerHandle.Clear();
                scroller.ScrollToVerticalOffset(0);
                descriptionContainer.Opacity = 1d;
            }
            else
            {
                dontSetNewsItem = true;
                descriptionContainer.Opacity = 0d;
            }

            if (this.currentNewsItem.IsFavorite)
            {
                toolbar.ToComplementFill();
            }
            else
            {
                toolbar.ToAccentFill();
            }
            
            Visibility = Visibility.Visible;

            PlayAnimations(verticalOffset, dontSetNewsItem);
        }

        void SubscribeToFullArticleCloseTap()
        {
            grid.GetSuperMouseLeftButtonTap()
                //.Merge(headerGrid.GetSuperMouseLeftButtonTap())
                            //.Merge(copy.GetMouseLeftButtonTap())
                .Where(_ => !IsCloseLocked)
                .Take(1)
                .Subscribe(OnFullArticleTap)
                .DisposeWith(openArticleDisposables);
        }

        void SubscribeToFlickGestures()
        {
            var gestureListener = GestureService.GetGestureListener(this.LayoutRoot);

            Observable.FromEventPattern<FlickGestureEventArgs>(
                e => gestureListener.Flick += e,
                e => gestureListener.Flick -= e)
                .Where(o => o.EventArgs.Direction == Orientation.Horizontal)
                .Select(o => o.EventArgs.HorizontalVelocity)
                .Subscribe(HandleFlick)
                .DisposeWith(openArticleDisposables);
        }

        void HandleFlick(double velocity)
        {
            if (velocity < -750)
                this.requestNextPage.OnNext();

            else if (velocity > 700)
                this.requestPreviousPage.OnNext();
        }


        void PlayAnimations(double verticalOffset, bool dontSetNewsItem)
        {
            var headerTransform = headerGrid.RenderTransform as CompositeTransform;

            var adjustedOffset = verticalOffset - this.spacerRow.Height.Value;
            headerTransform.TranslateY = adjustedOffset;

            if (!dontSetNewsItem)
                descriptionContainer.Children.Clear();

            //if (!descriptionContainer.Children.Contains(headerGrid))
            //    descriptionContainer.Children.Add(headerGrid);

            grid.Height = 728d;

            var mediaTypeImageBrush = currentNewsItem.GetMediaTypeImageBrush();
            if (mediaTypeImageBrush != null)
            {
                mediaTypesSpacer.Visibility = Visibility.Visible;
            }
            else
            {
                mediaTypesSpacer.Visibility = Visibility.Collapsed;
            }

            HeaderFadeOutSideBarAndImageSB.Begin();


            HeaderSlideToTopSB.Begin();
            
            Observable.Timer(TimeSpan.FromSeconds(0.35), DispatcherScheduler.Instance).Take(1)//.BeginWithNotification()
                .Subscribe(o =>
                {
                    grid.Height = 656d;

                    HeaderTextWidthTransformationSB.BeginWithNotification().Subscribe(_ =>
                    {
                        IsCloseLocked = false;

                        SubscribeToFullArticleCloseTap();
                        SubscribeToFlickGestures();

                        if (!dontSetNewsItem)
                            SetArticleContent();
                        else
                        {
                            ContentFadeInSB.Begin();
                        }

                        this.toolbar.Visibility = Visibility.Visible;
                        this.toolbar.Show();


                        if (AdVisibilityService.AreAdsStillBeingShownAtAll)
                        {
                            adControl.ShowAdControl();
                        }
                    })
                    .DisposeWith(this.openArticleDisposables);
                })
                .DisposeWith(this.openArticleDisposables);

            //if (AdVisibilityService.AreAdsStillBeingShownAtAll)
            //{
            //    showNewsItemAnimationCompleted.Take(1)
            //        .Subscribe(notUsed => adControl.ShowAdControl())
            //        .DisposeWith(openArticleDisposables);
            //}
        }



        void SetArticleContent()
        {
            this.setArticleContentTimerHandle.Clear();

            if (this.currentNewsItem == null)
                return;


            var paragraphs = new[] { "temp" }; //RssServiceLayer.Sanitize(this.currentNewsItem.Description);


            AddPlayVideoButtonIfYoutubeOrMp4Present();
            AddPlayAudioButtonIfAudioPresent();

            if ((this.descriptionContainer.Children.Contains(this.playVideoButton) || this.descriptionContainer.Children.Contains(this.playAudioButton))
                && !this.descriptionContainer.Children.Contains(this.mediaButtonSpacer))
            {
                this.descriptionContainer.Children.Add(this.mediaButtonSpacer);
            }


            if (!descriptionContainer.Children.Contains(image))
                descriptionContainer.Children.Add(image);


            if (this.currentNewsItem.HasImage)
            {
                image.Opacity = 0;

                var bitmap = new BitmapImage(new Uri(this.currentNewsItem.ImageUrl)) { CreateOptions = BitmapCreateOptions.BackgroundCreation };
                bitmap.ImageOpenedOrFailed().SafelySubscribe(() =>
                {
                    image.Visibility = Visibility.Visible;
                    var sb = image.Fade().To(1d).Over(0.5.Seconds()).ToStoryboard();
                    this.setArticleContentTimerHandle.Add(Disposable.Create(() => sb.Stop()));
                    sb.Begin();
                })
                .DisposeWith(this.setArticleContentTimerHandle);

                image.Source = bitmap;
                
                //ImageCache
                //    .GetImageAsync(this.currentNewsItem.ImageUrl)
                //    .SafelySubscribe(
                //    o =>
                //    {
                //        image.Source = o;
                //        image.Visibility = Visibility.Visible;
                //        var sb = image.Fade().To(1d).Over(0.5.Seconds()).ToStoryboard();
                //        this.setArticleContentTimerHandle.Add(Disposable.Create(() => sb.Stop()));
                //        sb.Begin();
                //    },
                //    ex => image.Source = null)
                //    .DisposeWith(this.setArticleContentTimerHandle);

                //image.Source = ImageCache.GetImage(this.currentNewsItem.ImageUrl);
                //image.Opacity = 0;
                //image.Visibility = Visibility.Visible;

                //image.Fade().To(1d).Over(0.5.Seconds()).ToStoryboard().Begin();
            }
            else
            {
                image.Visibility = Visibility.Collapsed;
                image.Source = null;
            }
            
            if (AdVisibilityService.AreAdsStillBeingShownAtAll)
                descriptionContainer.Children.Add(adControl);

            int textBlockCount = 0;

            paragraphs.IntroducePeriod(TimeSpan.FromMilliseconds(33))
                .ObserveOnDispatcher()
                .SafelySubscribe(
                    o =>
                    {
                        //var tb = new TextBlock { Text = o, Style = dtbs, IsHitTestVisible = false };
                        //tb.SetBinding(TextBlock.FontSizeProperty, bindingSource.DescriptionSizeBinding);
                        //tb.SetBinding(TextBlock.FontFamilyProperty, bindingSource.ThicknessBinding);
                        //tb.SetBinding(TextBlock.LineHeightProperty, bindingSource.LineHeightBinding);

                        var tb = tbFactory.GetNewOrExisting();
                        tb.Text = o;

                        descriptionContainer.Children.Add(tb);

                        if (textBlockCount++ < 3)
                        {
                            tb.Opacity = 0d;
                            tb.Fade().To(0.93).Over(0.3.Seconds()).ToStoryboard().Begin();
                        }

                        Disposable.Create(() => tbFactory.Add(tb)).DisposeWith(this.setArticleContentTimerHandle);
                    },
                    exception => { ; },
                    () =>
                    {
                        if (!descriptionContainer.Children.Contains(filler))
                            descriptionContainer.Children.Add(filler);
                    })
                .DisposeWith(this.setArticleContentTimerHandle);
        }




        #region Check for media (youtube, videos, podcast, zune app ids) and add buttons if necessary

        void AddPlayVideoButtonIfYoutubeOrMp4Present()
        {
            if (
                (currentNewsItem.YoutubeId.IsNotNullOrEmpty() || currentNewsItem.VideoUri.IsNotNullOrEmpty())
                && !descriptionContainer.Children.Contains(this.playVideoButton))
            {
                this.playVideoButton.Foreground = this.currentNewsItem.IsFavorite ?
                    AppSettings.Palette.ComplementaryBrush
                    :
                    AppSettings.Palette.AccentBrush;

                descriptionContainer.Children.Add(this.playVideoButton);

                var sb = playVideoButton.Fade().From(0d).To(1d).Over(0.5.Seconds()).ToStoryboard();
                this.setArticleContentTimerHandle.Add(Disposable.Create(() => sb.Stop()));
                sb.Begin();


                if (currentNewsItem.YoutubeId.IsNotNullOrEmpty())
                {
                    this.playVideoButton.GetClick().Subscribe(() =>
                        new Microsoft.Phone.Tasks.WebBrowserTask
                        {
                            URL = string.Format(
                                "vnd.youtube:{0}?vndapp=youtube_mobile&vndclient=mv-google&vndel=watch",
                                this.currentNewsItem.YoutubeId)
                        }.Show())
                    .DisposeWith(this.setArticleContentTimerHandle);
                }

                else // its an mp4 then
                {
                    this.playVideoButton.GetClick().Subscribe(() =>
                        new Microsoft.Phone.Tasks.MediaPlayerLauncher
                        {
                            Media = new Uri(currentNewsItem.VideoUri, UriKind.Absolute),
                            Controls = Microsoft.Phone.Tasks.MediaPlaybackControls.All,
                        }.Show())
                    .DisposeWith(this.setArticleContentTimerHandle);
                }
            }
        }

        void AddPlayAudioButtonIfAudioPresent()
        {
            if (currentNewsItem.PodcastUri.IsNotNullOrEmpty() && !descriptionContainer.Children.Contains(this.playAudioButton))
            {
                this.playAudioButton.Foreground = this.currentNewsItem.IsFavorite ?
                    AppSettings.Palette.ComplementaryBrush
                    :
                    AppSettings.Palette.AccentBrush;

                descriptionContainer.Children.Add(this.playAudioButton);

                var sb = playAudioButton.Fade().From(0d).To(1d).Over(0.5.Seconds()).ToStoryboard();
                this.setArticleContentTimerHandle.Add(Disposable.Create(() => sb.Stop()));
                sb.Begin();

                this.playAudioButton.GetClick().Subscribe(() =>
                        new Microsoft.Phone.Tasks.MediaPlayerLauncher
                        {
                            Media = new Uri(currentNewsItem.PodcastUri, UriKind.Absolute),
                            Controls = Microsoft.Phone.Tasks.MediaPlaybackControls.All,
                        }.Show())
                .DisposeWith(this.setArticleContentTimerHandle);
            }
        }

        #endregion





        #region Set the Article header (the part at top)

        void SetArticleHeader()
        {
            if (this.currentNewsItem == null)
                return;

            title.Text = this.currentNewsItem.Title;
            feedName.Text = this.currentNewsItem.FormattedForMainPageSourceAndDate;
            //publishedDateOverlay.Text = this.currentNewsItem.FeedSource.FeedName;

            if (this.currentNewsItem.HasImage)
            {
                this.textGrid.Width = 300d;

                imageRect.Visibility = imageHeaderWrap.Visibility = Visibility.Visible;

                imageBrush.ImageSource = new BitmapImage(new Uri(currentNewsItem.ImageUrl)) { CreateOptions = BitmapCreateOptions.None };
                //ImageCache
                //    .GetImageAsync(this.currentNewsItem.ImageUrl)
                //    .SafelySubscribe(
                //    o =>
                //    {
                //        imageBrush.ImageSource = o;
                //    },
                //    ex => imageBrush.ImageSource = null)
                //    .DisposeWith(this.openArticleDisposables);
            }
            else
            {
                this.textGrid.Width = 432d;

                imageRect.Visibility = imageHeaderWrap.Visibility = Visibility.Collapsed;
                imageBrush.ImageSource = null;
            }


            Binding b = new Binding("HasBeenViewed")
            {
                Converter = new DelegateValueConverter(value =>
                {
                    var hasBeenViewed = (bool)value;
                    return hasBeenViewed ? 0.5d : 1d;
                },
                    null),
                Source = this.currentNewsItem
            };

            title.SetBinding(UIElement.OpacityProperty, b);
            //timeAndSourceWrapPanel.SetBinding(UIElement.OpacityProperty, b);
            if (this.currentNewsItem.HasImage)
                imageRect.SetBinding(UIElement.OpacityProperty, b);

            if (this.currentNewsItem.IsFavorite)
            {
                feedName.Foreground = AppSettings.Palette.ComplementaryBrush;
                //publishedDateOverlay.Foreground = AppSettings.Palette.ComplementaryBrush;
            }
            else
            {
                feedName.Foreground = AppSettings.Palette.AccentBrush;
                //publishedDateOverlay.Foreground = AppSettings.Palette.AccentBrush;
            }
        }

        #endregion




        public void Hide()
        {
            this.openArticleDisposables.Clear();

            HeaderSlideToTopSB.Pause();
            HeaderFadeOutSideBarAndImageSB.Pause();
            HeaderTextWidthTransformationSB.Pause();
            ContentFadeInSB.Pause();

            hideStarted.OnNext(new Unit());
            DropOutSB.BeginWithNotification().Subscribe(o =>
            {
                Visibility = Visibility.Collapsed;
                hideCompleted.OnNext(new Unit());
                DropOutSB.Stop();
            })
            .DisposeWith(this.closeArticleDisposables);
        }

        enum SwitchDirection
        {
            Next,
            Previous
        }

        void SwitchArticle(SwitchDirection direction)
        {
            if (this.currentNewsItem == null)
                return;

            this.setArticleContentTimerHandle.Clear();

            scroller.ScrollToVerticalOffset(0);

            title.Text = this.currentNewsItem.Title;
            feedName.Text = this.currentNewsItem.FormattedForMainPageSourceAndDate;
            //publishedDateOverlay.Text = this.currentNewsItem.FeedSource.FeedName;

            if (AppSettings.PermanentState.DoesOpeningAnArticleMarkItReadAutomatically)
                this.currentNewsItem.HasBeenViewed = true;

            Binding b = new Binding("HasBeenViewed")
            {
                Converter = new DelegateValueConverter(value =>
                {
                    var hasBeenViewed = (bool)value;
                    return hasBeenViewed ? 0.5d : 1d;
                },
                    null),
                Source = this.currentNewsItem
            };

            title.SetBinding(UIElement.OpacityProperty, b);
            //timeAndSourceWrapPanel.SetBinding(UIElement.OpacityProperty, b);

            if (this.currentNewsItem.IsFavorite)
            {
                feedName.Foreground = AppSettings.Palette.ComplementaryBrush;
            }
            else
            {
                feedName.Foreground = AppSettings.Palette.AccentBrush;
            }


            if (direction == SwitchDirection.Next)
                NextArticleSB2.Begin();

            else if (direction == SwitchDirection.Previous)
                PreviousArticleSB2.Begin();

            else
                return;

            descriptionContainer.Children.Clear();

            //if (!descriptionContainer.Children.Contains(headerGrid))
            //    descriptionContainer.Children.Add(headerGrid);

            Observable.Timer(0.2d.Seconds(), DispatcherScheduler.Instance).Take(1)//.ObserveOnDispatcher()//observable
                .Subscribe(notUsed =>
                {
                    SetArticleContent();
                })
                .DisposeWith(this.setArticleContentTimerHandle);

            if (this.currentNewsItem.IsFavorite && this.lastNewsItem != null && !this.lastNewsItem.IsFavorite)
            {
                toolbar.ToComplementFill(true);
            }
            else if (!this.currentNewsItem.IsFavorite && this.lastNewsItem != null && this.lastNewsItem.IsFavorite)
            {
                toolbar.ToAccentFill(true);
            }
        }

        public void NextArticle(NewsItem newsItem)
        {
            this.lastNewsItem = this.currentNewsItem;
            this.currentNewsItem = newsItem;

            this.NextArticleSB1.BeginWithNotification().Subscribe(_ => 
            {
                this.NextArticleSB1.Stop();
                SwitchArticle(SwitchDirection.Next);
            });
        }

        public void PreviousArticle(NewsItem newsItem)
        {
            this.lastNewsItem = this.currentNewsItem;
            this.currentNewsItem = newsItem;

            this.PreviousArticleSB1.BeginWithNotification().Subscribe(_ =>
            {
                this.PreviousArticleSB1.Stop();
                SwitchArticle(SwitchDirection.Previous);
            });
        }

        void MarkCurrentArticleRead()
        {
            if (this.currentNewsItem != null)
                this.currentNewsItem.HasBeenViewed = true;
            Hide();
        }

        void OnFullArticleTap()
        {
            if (this.toolbar.IsExtended)
                this.toolbar.CloseExtended(true);

            else
                Hide();
        }

        void OnViewArticleButtonClicked()
        {
            if (this.currentNewsItem == null)
                return;

            this.currentNewsItem.HasBeenViewed = true;
            GlobalNavigationService.ToWebBrowserPage(this.currentNewsItem);
            Hide();
        }

        void OnShareButtonClicked()
        {
            ShowShareMenu();
        }

        void OnMarkReadButtonClicked()
        {
            MarkCurrentArticleRead();
        }

        public void Dispose()
        {
            this.disposables.Dispose();
            this.openArticleDisposables.Dispose();
            this.closeArticleDisposables.Dispose();
            this.tbFactory.Dispose();

            using (this.adControl)
            {
                this.adControl = null;
            }
        }

        ~ArticleView()
        {
            DebugEx.WriteLine("ArticleView {0} destroyed", id.ToString());
        }

        class DescriptionTextBlockFactory : IDisposable
        {
            Stack<TextBlock> existingTBs = new Stack<TextBlock>();
            BindableMainPageFontStyle bindingSource;
            Style dtbs;

            internal DescriptionTextBlockFactory(Style dtbs)
            {
                bindingSource = ServiceResolver.Get<BindableMainPageFontStyle>();
                this.dtbs = dtbs;
            }

            internal TextBlock GetNewOrExisting()
            {
                if (existingTBs.Count > 0)
                {
                    //DebugEx.WriteLine("using an EXISTING textblock");
                    return existingTBs.Pop();
                }
                else
                {
                    //DebugEx.WriteLine("creating a new textblock");
                    return GenerateTextBlock();
                }
            }

            internal void Add(TextBlock t)
            {
                existingTBs.Push(t);
            }

            internal void Add(IEnumerable<TextBlock> textBlocks)
            {
                foreach (var t in textBlocks)
                    existingTBs.Push(t);
            }

            TextBlock GenerateTextBlock()
            {
                var tb = new TextBlock 
                { 
                    Style = dtbs,
                    IsHitTestVisible = false,
                };
                tb.SetBinding(TextBlock.FontSizeProperty, bindingSource.DescriptionSizeBinding);
                tb.SetBinding(TextBlock.FontFamilyProperty, bindingSource.ThicknessBinding);
                tb.SetBinding(TextBlock.LineHeightProperty, bindingSource.LineHeightBinding);
                return tb;
            }

            public void Dispose()
            {
                existingTBs.Clear();
            }
        }
    }
}