using SelesGames;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Storage;
using Windows.System.Threading;

namespace weave
{
    public partial class BigImageNewsItemControl : BaseNewsItemControl, IDisposable
    {
        BindableMainPageFontStyle bindingSource;
        SerialDisposable disp = new SerialDisposable();
        ImageHelper imageHelper = new ImageHelper();

        public BigImageNewsItemControl()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;
            bitmap.CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation;
            title.Text = null;
            feedName.Text = null;
            mediaTypesIcon.OpacityMask = null;
            ClearExistingImage();

            bindingSource = ServiceResolver.Get<BindableMainPageFontStyle>();

            this.title.SetBinding(TextBlock.FontSizeProperty, bindingSource.TitleSizeBinding);
            this.title.SetBinding(TextBlock.FontFamilyProperty, bindingSource.ThicknessBinding);

            this.feedName.SetBinding(TextBlock.FontSizeProperty, bindingSource.PublicationLineSizeBinding);
            this.feedName.SetBinding(TextBlock.FontFamilyProperty, bindingSource.ThicknessBinding);

            this.SetBinding(FrameworkElement.MarginProperty, bindingSource.MainPageNewsItemMarginBinding);
        }

        protected override void SetNewsItem(NewsItem newsItem)
        {
            disp.Disposable = null;
            var disposables = new CompositeDisposable();
            disp.Disposable = disposables;

            OnLoadSB.Stop();
            OnLoadBackwardsSB.Stop();
            ImageFadeInSB.Stop();

            ClearExistingImage();

            title.Text = newsItem.Title;
            feedName.Text = newsItem.FormattedForMainPageSourceAndDate;//newsItem.FeedSource.FeedName;

            var mediaTypeImageBrush = newsItem.GetMediaTypeImageBrush();
            if (mediaTypeImageBrush != null)
            {
                mediaTypesIcon.OpacityMask = mediaTypeImageBrush;
                mediaTypesIcon.Visibility = Visibility.Visible;
            }
            else
            {
                mediaTypesIcon.Visibility = Visibility.Collapsed;
            }

            if (newsItem.HasImage)
            {
                //image.Opacity = 0;
                imageWrapper.Visibility = Visibility.Visible;

                //ImageCache
                //    .GetImageAsync(newsItem.ImageUrl)
                //    .SafelySubscribe(SetImage, ex => SetImage(FailImage))
                //    .DisposeWith(disposables);
                //var bi = new BitmapImage();// { CreateOptions = BitmapCreateOptions.BackgroundCreation };
                //image.Source = bi;
                //imageHelper.LoadImage(newsItem.ImageUrl, bi);
                bitmap.UriSource = new Uri(newsItem.ImageUrl, UriKind.Absolute);
            }
            else
            {
                imageWrapper.Visibility = Visibility.Collapsed;
            }

            Binding b = new Binding("DisplayState")
            {
                Converter = new DelegateValueConverter(value =>
                    {
                        var displayState = (weave.NewsItem.ColoringDisplayState)value;
                        return (displayState == weave.NewsItem.ColoringDisplayState.Viewed) ? 0.6d : 1d;
                    },
                    null),
                Source = newsItem
            };

            this.grid.SetBinding(UIElement.OpacityProperty, b);
            this.imageWrapper.SetBinding(UIElement.OpacityProperty, b);

            ColorByline(newsItem);

            Observable.FromEventPattern<PropertyChangedEventArgs>(newsItem, "PropertyChanged")
                .Where(o => o.EventArgs.PropertyName == "DisplayState")
                .SafelySubscribe(o => ColorByline(newsItem))
                .DisposeWith(disposables);
        }

        void ColorByline(NewsItem newsItem)
        {
            if (newsItem == null || feedName == null || mediaTypesIcon == null)
                return;

            Brush brush;

            if (newsItem.IsFavorite)
                brush = AppSettings.Instance.Themes.CurrentTheme.ComplementaryBrush;
            else if (newsItem.IsNew())
                brush = AppSettings.Instance.Themes.CurrentTheme.AccentBrush;
            else
                brush = AppSettings.Instance.Themes.CurrentTheme.SubtleBrush;

            feedName.Foreground = brush;
            mediaTypesIcon.Fill = brush;
        }

        void ClearExistingImage()
        {
            //if (image.Source != null && image.Source is BitmapImage)
            //{
            //    var bi = (BitmapImage)image.Source;
            //    bi.UriSource = null;
            //    bi = null;
            //    image.Source = null;
            //}
        }

        //void SetImage(ImageSource image)
        //{
        //    this.ImageFadeInSB.Stop();
        //    this.image.Source = image;
        //    this.ImageFadeInSB.Begin();
        //}

        public override void PageRight()
        {
            OnLoadSB.Begin();
        }

        public override void PageLeft()
        {
            OnLoadBackwardsSB.Begin();
        }

        public void Dispose()
        {
            disp.Dispose();
        }
    }

    public class ImageHelper
    {
        readonly string IMAGE_TEMP_FOLDER = "images";
        readonly Dispatcher dispatcher;

        public ImageHelper()
        {
            dispatcher = Deployment.Current.Dispatcher;
        }

        public void LoadImage(string url, BitmapImage bi)
        {
            string tempJPEG = url.Substring(url.LastIndexOf("/") + 1);

            Task.Run(() =>

            LoadFromLocalStorage(tempJPEG, bi)
                .ContinueWith(_ => DownloadAsync(url, tempJPEG, bi)
                    .ContinueWith(__ => SetFailImage(bi), TaskContinuationOptions.NotOnRanToCompletion), TaskContinuationOptions.NotOnRanToCompletion));
        }

        void SetFailImage(BitmapImage bitmapImage)
        {
            dispatcher.BeginInvoke(() =>
                bitmapImage.UriSource = new Uri("/weave;component/Assets/imageDownloadFailed.jpg", UriKind.Relative));
        }
        


        #region LoadFromLocalStorage - read stream from local storage if possible

        async Task LoadFromLocalStorage(string fileName, BitmapImage bitmapImage)
        {
            //DebugEx.WriteLine("1 now we on dis thread");
            StorageFolder tilesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMAGE_TEMP_FOLDER, CreationCollisionOption.OpenIfExists);
            //DebugEx.WriteLine("2 now we on dis thread");
            var file = await tilesFolder.GetFileAsync(fileName);
            //DebugEx.WriteLine("3 now we on dis thread");
            using (var inputStream = await file.OpenSequentialReadAsync())
            using (var stream = inputStream.AsStreamForRead())
            {
                //DebugEx.WriteLine("2 now we on dis thread");
                await dispatcher.InvokeAsync(() => bitmapImage.SetSource(stream));
            }
        }

        #endregion




        #region SaveToLocalStorage - write image stream to local storage

        async Task SaveToLocalStorage(string fileName, Stream imageStream)
        {
            StorageFolder tilesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMAGE_TEMP_FOLDER, CreationCollisionOption.OpenIfExists);
            var file = await tilesFolder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

            using (StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync())
            using (var stream = transaction.Stream.AsStreamForWrite())
            {
                await imageStream.CopyToAsync(stream).ConfigureAwait(false);
                await transaction.CommitAsync();
            }
        }

        #endregion




        Task DownloadAsync(string imageUrl, string fileName, BitmapImage bitmapImage)
        {
            //WebRequest.RegisterPrefix("http://", System.Net.Browser.WebRequestCreator.ClientHttp);

            var request = (HttpWebRequest)System.Net.Browser.WebRequestCreator.ClientHttp.Create(new Uri(imageUrl, UriKind.Absolute));
            return request.GetResponseAsync().ContinueWith(async responseTask =>
            {
                var response = responseTask.Result;
                using (var stream = response.GetResponseStream())
                {
                    var length = (int)stream.Length;
                    var buffer = new byte[length];
                    await stream.ReadAsync(buffer, 0, length).ConfigureAwait(false);
                    var ms = new MemoryStream(buffer);
                    await dispatcher.InvokeAsync(() => bitmapImage.SetSource(ms));
                    ms = new MemoryStream(buffer);
                    await SaveToLocalStorage(fileName, ms);
                }
            },
            TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}