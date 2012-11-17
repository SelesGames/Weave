using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace weave
{
    public class NewsItemControl : Control
    {
        static BitmapImage failImage;

        TextBlock Title, FeedName;
        FrameworkElement LayoutRoot;
        UIElement ImageWrapper;
        Image Image;
        Shape MediaTypesIcon;
        Storyboard OnLoadSB, OnLoadBackwardsSB, ImageFadeInSB;
        SerialDisposable disp = new SerialDisposable();
        bool isTemplateApplied = false;


        public NewsItemControl()
        {
            DefaultStyleKey = typeof(NewsItemControl);

            if (this.IsInDesignMode())
            {
                ImageCache = new ImageCache();
                return;
            }

            if (failImage == null)
                failImage = new BitmapImage(new Uri("/weave;component/Assets/imageDownloadFailed.png", UriKind.Relative));

            NewsItemSelected = this.GetTap().Where(_ => this.NewsItem != null).Select(notUsed => Tuple.Create<object, NewsItem>(this, this.NewsItem));
        }

        public IObservable<Tuple<object, NewsItem>> NewsItemSelected { get; private set; }
        public ImageCache ImageCache { get; set; }

        public override void OnApplyTemplate()
        {
            //base.OnApplyTemplate();

            LayoutRoot = base.GetTemplateChild("LayoutRoot") as FrameworkElement;
            Title = base.GetTemplateChild("Title") as TextBlock;
            FeedName = base.GetTemplateChild("FeedName") as TextBlock;
            ImageWrapper = base.GetTemplateChild("ImageWrapper") as UIElement;
            Image = base.GetTemplateChild("Image") as Image;
            MediaTypesIcon = base.GetTemplateChild("MediaTypesIcon") as Shape;

            CreateAnimations();

            isTemplateApplied = true;

            if (NewsItem != null)
                SetNewsItem(NewsItem);
        }

        void SetNewsItem(NewsItem newsItem)
        {
            if (!isTemplateApplied || newsItem == null)
                return;

            disp.Disposable = null;

            OnLoadSB.Stop();
            OnLoadBackwardsSB.Stop();
            ImageFadeInSB.Stop();
            ClearExistingImage();

            var brush = newsItem.IsNew ? AppSettings.Instance.Themes.CurrentTheme.AccentBrush : AppSettings.Instance.Themes.CurrentTheme.SubtleBrush;

            if (Title != null) 
                Title.Text = newsItem.Title;

            if (FeedName != null)
            {
                FeedName.Text = newsItem.FormattedForMainPageSourceAndDate;
                FeedName.Foreground = brush;
            }

            if (MediaTypesIcon != null)
            {
                var mediaTypeImageBrush = newsItem.GetMediaTypeImageBrush();
                if (mediaTypeImageBrush != null)
                {
                    MediaTypesIcon.OpacityMask = mediaTypeImageBrush;
                    MediaTypesIcon.Visibility = Visibility.Visible;
                    MediaTypesIcon.Fill = brush;
                }
                else
                {
                    MediaTypesIcon.Visibility = Visibility.Collapsed;
                }
            }

            if (Image != null && ImageCache != null && newsItem.HasImage)
            {
                Image.Opacity = 0;
                disp.Disposable = ImageCache
                    .GetImageAsync(newsItem.ImageUrl)
                    .SafelySubscribe(SetImage, ex => SetImage(failImage));
            }

            if (ImageWrapper != null)
                ImageWrapper.Visibility = newsItem.HasImage ? Visibility.Visible : Visibility.Collapsed;

            Binding b = new Binding("HasBeenViewed")
            {
                Converter = new DelegateValueConverter(value =>
                {
                    var hasBeenViewed = (bool)value;
                    return hasBeenViewed ? 0.6d : 1d;
                },
                    null),
                Source = newsItem
            };

            if (LayoutRoot != null)
                LayoutRoot.SetBinding(UIElement.OpacityProperty, b);
            else
                this.SetBinding(UIElement.OpacityProperty, b);
        }




        #region Image helper functions

        void ClearExistingImage()
        {
            if (Image != null && Image.Source is BitmapImage)
            {
                var bi = (BitmapImage)Image.Source;
                bi.UriSource = null;
                bi = null;
                Image.Source = null;
            }
        }

        void SetImage(ImageSource image)
        {
            if (ImageFadeInSB == null || Image == null || image == null)
                return;

            ImageFadeInSB.Stop();
            Image.Source = image;
            ImageFadeInSB.Begin();
        }

        #endregion




        #region Animation Helper Functions

        internal void PlaySlideAnimation(int direction)
        {
            if (!isTemplateApplied || direction == 99)
                return;

            if (direction >= 0)
                OnLoadSB.Begin();
            else
                OnLoadBackwardsSB.Begin();
        }

        void CreateAnimations()
        {
            DoubleAnimation da;

            if (Image != null)
            {
                ImageFadeInSB = new Storyboard();
                da = new DoubleAnimation { Duration = 0.5.Seconds(), From = 0d, To = 1d };
                Storyboard.SetTarget(da, Image);
                Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
                ImageFadeInSB.Children.Add(da);
            }

            RenderTransformOrigin = new Point(0.5, 0.5);
            if (!(RenderTransform is CompositeTransform))
                RenderTransform = new CompositeTransform();

            OnLoadSB = new Storyboard();
            da = new DoubleAnimation { Duration = 0.5.Seconds(), From = 480d, To = 0d, EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(da, (CompositeTransform)RenderTransform);
            Storyboard.SetTargetProperty(da, new PropertyPath(CompositeTransform.TranslateXProperty));
            OnLoadSB.Children.Add(da);

            OnLoadBackwardsSB = new Storyboard();
            da = new DoubleAnimation { Duration = 0.5.Seconds(), From = -480d, To = 0d, EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(da, (CompositeTransform)RenderTransform);
            Storyboard.SetTargetProperty(da, new PropertyPath(CompositeTransform.TranslateXProperty));
            OnLoadBackwardsSB.Children.Add(da);
        }

        #endregion




        #region Dependency Properties (NewsItem)

        public static readonly DependencyProperty NewsItemProperty = DependencyProperty.Register(
            "NewsItem",
            typeof(NewsItem),
            typeof(NewsItemControl),
            new PropertyMetadata(OnNewsItemChanged));

        public NewsItem NewsItem
        {
            get { return (NewsItem)GetValue(NewsItemProperty); }
            set { SetValue(NewsItemProperty, value); }
        }

        static void OnNewsItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var ui = s as NewsItemControl;
            if (ui == null)
                return;

            if (e.NewValue is NewsItem)// && e.NewValue != e.OldValue)
            {
                var newsItem = e.NewValue as NewsItem;
                ui.SetNewsItem(newsItem);
            }
        }

        public static readonly DependencyProperty BylineFontSizeProperty = DependencyProperty.Register(
            "BylineFontSize",
            typeof(double),
            typeof(NewsItemControl),
            new PropertyMetadata(24d));

        public double BylineFontSize
        {
            get { return (double)GetValue(BylineFontSizeProperty); }
            set { SetValue(BylineFontSizeProperty, value); }
        }

        #endregion




        public void Dispose()
        {
            disp.Dispose();
        }
    }
}
