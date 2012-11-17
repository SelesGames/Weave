using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Weave.Customizability;

namespace weave
{
    public partial class CustomList : UserControl, IDisposable
    {
        List<BaseNewsItemControl> newsItemsUI;
        SerialDisposable disp = new SerialDisposable();
        PermanentState permState;


        public IObservable<Tuple<object, NewsItem>> NewsItemSelected { get; private set; }

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

            if (this.IsInDesignMode())
                return;

            id = Guid.NewGuid();
            DebugEx.WriteLine("CustomList {0} created", id.ToString());

            this.sp.Children.Remove(this.bottomButtons);

            scroller.Visibility = Visibility.Collapsed;
            permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();
        }

        internal void CompleteInitialization()
        {
            newsItemsUI =
                Enumerable.Range(0, AppSettings.Instance.NumberOfNewsItemsPerMainPage)
                .Select(notUsed =>
                {
                    var ui = CreateNewsItemControl();
                    ui.Visibility = Visibility.Collapsed;
                    sp.Children.Add(ui);
                    return (BaseNewsItemControl)ui;
                })
                .ToList();

            this.sp.Children.Add(this.bottomButtons);

            NewsItemSelected = newsItemsUI.Select(o => o.GetTap().Select(_ => Tuple.Create((object)o, o.NewsItem))).Merge();
        }

        BaseNewsItemControl CreateNewsItemControl()
        {
            switch (permState.ArticleListFormat)
            {
                case ArticleListFormatType.BigImage:
                    return new BigImageNewsItemControl();

                case ArticleListFormatType.SmallImage:
                    return new MainPageNewsItemUI();

                default:
                    throw new Exception(string.Format(
                        "Selected ArticleListFormat is not currently supported: {0}", 
                        permState.ArticleListFormat.ToString()));
            }
        }

        public void SetNews(List<NewsItem> news, int direction)
        {
            disp.Disposable = null;

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


            firstX.IntroducePeriod(TimeSpan.Zero, animationDelay, DispatcherScheduler.Instance).SafelySubscribe(
                o =>
                {
                    o.ui.NewsItem = o.newsItem;
                    o.ui.Visibility = Visibility.Visible;
                    PlaySlideAnimation(o.ui, direction);
                },
                ex =>
                {
                    this.IsHitTestVisible = true;
                },
                () =>
                {
                    try
                    {
                        remainderX.IntroducePeriod(TimeSpan.Zero, TimeSpan.FromMilliseconds(12d), DispatcherScheduler.Instance).SafelySubscribe(
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

        void PlaySlideAnimation(BaseNewsItemControl baseNewsItemControl, int direction)
        {
                if (direction == 99)
                    return;

                if (direction >= 0)
                    baseNewsItemControl.PageRight();
                else
                    baseNewsItemControl.PageLeft();
        }

        public void SetImageCache(ImageCache cache)
        {
            if (newsItemsUI == null)
                return;

            foreach (var o in newsItemsUI)
                o.ImageCache = cache;
        }

        public void Dispose()
        {
            if (newsItemsUI != null)
            {
                foreach (var o in newsItemsUI)
                    using (var x = o as IDisposable) { }// o.Dispose();
            }
            disp.Dispose();
        }

        ~CustomList()
        {
            DebugEx.WriteLine("CustomList {0} destroyed", id.ToString());
        }
    }
}