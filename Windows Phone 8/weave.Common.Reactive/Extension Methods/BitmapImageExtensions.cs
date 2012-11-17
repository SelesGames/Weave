using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace System.Windows.Media.Imaging
{
    public static class BitmapImageExtensions
    {
        //static List<Uri> availableImages = new List<Uri>();
        //static List<Uri> erroredImages = new List<Uri>();

        //public static IObservable<Unit> GetImageAvailable(this BitmapImage bitmap)
        //{
        //    if (bitmap == null)
        //        throw new ArgumentNullException("bitmap");

        //    return Observable.Create<Unit>(observer =>
        //    {
        //        CompositeDisposable disposables = new CompositeDisposable();

        //        try
        //        {
        //            if (availableImages.Contains(bitmap.UriSource))
        //            {
        //                observer.OnNext();
        //                observer.OnCompleted();
        //            }
        //            else if (erroredImages.Contains(bitmap.UriSource))
        //            {
        //                observer.OnError(new Exception("image no there"));
        //            }
        //            else
        //            {
        //                //Observable.FromEventPattern<RoutedEventArgs>(bitmap, "ImageOpened")
        //                //    .Take(1)
        //                //    .Subscribe(_ =>
        //                //    {
        //                //        //add to cache
        //                //        availableImages.Add(bitmap.UriSource);
        //                //        observer.OnNext();
        //                //        observer.OnCompleted();
        //                //    })
        //                //    .DisposeWith(disposables);

        //                //Observable.FromEventPattern<ExceptionRoutedEventArgs>(bitmap, "ImageFailed")
        //                //    .Take(1)
        //                //    .Subscribe(arg =>
        //                //    {
        //                //        erroredImages.Add(bitmap.UriSource);
        //                //        observer.OnError(arg.EventArgs.ErrorException);
        //                //    })
        //                //    .DisposeWith(disposables);

        //                Observable.FromEventPattern<DownloadProgressEventArgs>(bitmap, "DownloadProgress")
        //                    .Where(o => o.EventArgs.Progress == 100)
        //                    .Take(1)
        //                    .Delay(TimeSpan.FromSeconds(0.1d), DispatcherScheduler.Instance)
        //                    .SafelySubscribe(() =>
        //                    {
        //                        availableImages.Add(bitmap.UriSource);
        //                        observer.OnNext();
        //                        observer.OnCompleted();
        //                    },
        //                    observer.OnError)
        //                    .DisposeWith(disposables);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            observer.OnError(ex);
        //        }
        //        return disposables;
        //    });
        //}


        public async static Task<ExceptionRoutedEventArgs> WaitForLoadedAsync(this BitmapImage bitmapImage)
        {
            var tcs = new TaskCompletionSource<ExceptionRoutedEventArgs>();

            EventHandler<RoutedEventArgs> reh = null;
            EventHandler<ExceptionRoutedEventArgs> ereh = null;

            reh = (s, e) =>
            {
                bitmapImage.ImageOpened -= reh;
                bitmapImage.ImageFailed -= ereh;
                tcs.SetResult(null);
            };


            ereh = (s, e) =>
            {
                bitmapImage.ImageOpened -= reh;
                bitmapImage.ImageFailed -= ereh;
                tcs.SetResult(e);
            };

            bitmapImage.ImageOpened += reh;
            bitmapImage.ImageFailed += ereh;

            return await tcs.Task;
        }

        public static IObservable<Unit> ImageOpenedOrFailed(this BitmapImage bitmap)
        {
            return Observable
                .FromEventPattern<RoutedEventArgs>(bitmap, "ImageOpened").Select(_ => Unit.Default)
                .Merge(Observable.FromEventPattern<ExceptionRoutedEventArgs>(bitmap, "ImageFailed").Select(_ => Unit.Default))
                .Take(1);
        }

        public class ImageOpenOrFailedResult
        {
            public bool IsFaulted { get; set; }
            public Exception Exception { get; set; }
        }

        public static IObservable<ImageOpenOrFailedResult> ImageOpenOrFailed(this ImageBrush imageBrush)
        {
            return Observable
                .FromEventPattern<RoutedEventArgs>(imageBrush, "ImageOpened").Select(o => new ImageOpenOrFailedResult { IsFaulted = false })
                .Merge(Observable.FromEventPattern<ExceptionRoutedEventArgs>(imageBrush, "ImageFailed").Select(o => new ImageOpenOrFailedResult { IsFaulted = true, Exception = o.EventArgs.ErrorException }))
                .Take(1);
        }
    }
}
