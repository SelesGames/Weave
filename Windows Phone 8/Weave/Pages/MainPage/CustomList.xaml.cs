using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Weave.Customizability;
using Weave.ViewModels;

namespace weave
{
    public partial class CustomList : UserControl, IDisposable
    {
        List<BaseNewsItemControl> newsItemsUI;
        SerialDisposable disp = new SerialDisposable();
        PermanentState permState;
        Subject<Tuple<object, NewsItem>> newsItemSelected = new Subject<Tuple<object, NewsItem>>();
        Brush transparentBrush;
        IDisposable tapHandle;
        IReadOnlyList<NewsItem> displayedNews;

        ImageCache imageCache;

        public IObservable<Tuple<object, NewsItem>> NewsItemSelected { get { return newsItemSelected.AsObservable(); } }

        Guid id;

        public bool IsPreviousIndicatorEnabled
        {
            set { prevIndicator.Opacity = value ? 1d : 0.5d; }
        }

        public bool IsNextIndicatorEnabled
        {
            set { nextIndicator.Opacity = value ? 1d : 0.5d; }
        }

        public CustomList()
        {
            InitializeComponent();
            transparentBrush = scroller.Background;

            if (this.IsInDesignMode())
                return;


            this.sp.Children.Remove(this.bottomButtons);

            scroller.Visibility = Visibility.Collapsed;
            permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();

            imageCache = Resources["imageCache"] as ImageCache;

            id = Guid.NewGuid();
            DebugEx.WriteLine("CustomList {0} created", id.ToString());
        }

        public int AnimationDirection { get; set; }




        #region Draw the BaseNewsItemControls to the StackPanel

        public void InitializeNewsItemControls()
        {
            ApplyCurrentTheme();

            // clean up subscription and dispose previous news items
            if (tapHandle != null)
                tapHandle.Dispose();

            this.sp.Children.Clear();

            if (newsItemsUI != null)
            {
                foreach (var newsItem in newsItemsUI)
                {
                    if (newsItem is IDisposable)
                        ((IDisposable)newsItem).Dispose();
                }
            }

            newsItemsUI =
                Enumerable.Range(0, AppSettings.Instance.NumberOfNewsItemsPerMainPage)
                .Select(notUsed =>
                {
                    var ui = CreateNewsItemControl();
                    ui.ImageCache = imageCache;
                    ui.Visibility = Visibility.Collapsed;
                    sp.Children.Add(ui);
                    return (BaseNewsItemControl)ui;
                })
                .ToList();

            this.sp.Children.Add(this.bottomButtons);
            
            tapHandle = newsItemsUI.Select(o => o.GetTap().Select(_ => Tuple.Create((object)o, o.NewsItem))).Merge()
                .Subscribe(newsItemSelected.OnNext, newsItemSelected.OnError, newsItemSelected.OnCompleted);
        }

        void ApplyCurrentTheme()
        {
            if (permState.ArticleListFormat == ArticleListFormatType.Card)
            {
                scroller.Background = new SolidColorBrush(Color.FromArgb(255, 237, 237, 237));
            }
            else
            {
                scroller.Background = transparentBrush;
            }
        }

        public void UpdateToCurrentTheme()
        {
            InitializeNewsItemControls();
            SetNews(this.displayedNews);
        }

        BaseNewsItemControl CreateNewsItemControl()
        {
            switch (permState.ArticleListFormat)
            {
                case ArticleListFormatType.BigImage:
                    return new BigImageNewsItemControl();

                case ArticleListFormatType.SmallImage:
                    return new MainPageNewsItemUI();

                case ArticleListFormatType.Card:
                    return new CardNewsItemControl();

                case ArticleListFormatType.TextOnly:
                    return new TextOnlyNewsItemControl();

                case ArticleListFormatType.Tiles:
                    return new TileNewsItemControl();

                default:
                    throw new Exception(string.Format(
                        "Selected ArticleListFormat is not currently supported: {0}", 
                        permState.ArticleListFormat.ToString()));
            }
        }

        #endregion




        #region Set NewsItem for each BaseNewsItemControl on the screen

        void SetNews(IReadOnlyList<NewsItem> news)
        {
            disp.Disposable = null;
            this.displayedNews = news;

            if (this.imageCache != null)
                imageCache.Flush();

            scroller.ScrollToVerticalOffset(0);

            if (this.displayedNews == null)
                return;

            this.IsHitTestVisible = false;

            var animationDelay = TimeSpan.FromSeconds(0.08);
            var animationBuffer = 4;

            scroller.Visibility = Visibility.Collapsed;

            newsItemsUI.ForEach(o =>
            {
                o.Visibility = Visibility.Collapsed;
                o.NewsItem = null;
            });

            this.bottomButtons.Visibility = Visibility.Collapsed;
            this.scroller.Visibility = Visibility.Visible;

            var tuples = newsItemsUI.Zip(news, (ui, newsItem) => new { ui, newsItem }).ToList();

            CompositeDisposable disposables = new CompositeDisposable();
            disp.Disposable = disposables;




            #region approach 2, with a double timer and ObserveOnDispatcher - WORKS GREAT AS OF 9/16/11


            var firstX = tuples.Take(animationBuffer).ToList();
            var remainderX = tuples.Skip(animationBuffer).ToList();


            firstX.IntroducePeriod(TimeSpan.Zero, animationDelay, DispatcherScheduler.Current).SafelySubscribe(
                o =>
                {
                    o.ui.NewsItem = o.newsItem;
                    o.ui.Visibility = Visibility.Visible;
                    PlaySlideAnimation(o.ui);
                },
                ex =>
                {
                    this.IsHitTestVisible = true;
                },
                () =>
                {
                    try
                    {
                        remainderX.IntroducePeriod(TimeSpan.Zero, TimeSpan.FromMilliseconds(12d), DispatcherScheduler.Current).SafelySubscribe(
                            o =>
                            {
                                o.ui.NewsItem = o.newsItem;
                                o.ui.Visibility = Visibility.Visible;
                            },
                            ex =>
                            {
                                this.bottomButtons.Visibility = Visibility.Visible;
                                this.IsHitTestVisible = true;
                            },
                            () =>
                            {
                                this.bottomButtons.Visibility = Visibility.Visible;
                                this.IsHitTestVisible = true;
                            })
                        .DisposeWith(disposables);
                    }
                    catch (Exception) { }
                })
                .DisposeWith(disposables);

            #endregion
        }

        #endregion




        void PlaySlideAnimation(BaseNewsItemControl baseNewsItemControl)
        {
                if (AnimationDirection == 99)
                    return;

                if (AnimationDirection >= 0)
                    baseNewsItemControl.PageRight();
                else
                    baseNewsItemControl.PageLeft();
        }

        public void Dispose()
        {
            if (newsItemsUI != null)
            {
                foreach (var o in newsItemsUI)
                    using (var x = o as IDisposable) { }// o.Dispose();
            }
            disp.Dispose();

            if (tapHandle != null)
                tapHandle.Dispose();

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

            DebugEx.WriteLine("CustomList {0} disposed", id.ToString());
        }

        ~CustomList()
        {
            DebugEx.WriteLine("CustomList {0} destroyed", id.ToString());
        }




        #region Dependency Properties (NewsItem)

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IList),
            typeof(CustomList),
            new PropertyMetadata(OnItemsSourceChanged));

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        static void OnItemsSourceChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var cl = s as CustomList;
            if (cl == null)
                return;

            if (e.NewValue is IReadOnlyList<NewsItem>)
            {
                var news = (IReadOnlyList<NewsItem>)e.NewValue;
                cl.SetNews(news);
            }
        }

        #endregion
    }
}