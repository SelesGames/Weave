using SelesGames.Common.Reactive;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace weave
{
    public class ImageCache : IDisposable
    {
        readonly BitmapImageCache cache;
        //readonly ImageStreamCache cache;

        public ImageCache()
            : this(16, 32) { }

        public ImageCache(int bmpCacheLimit, int streamCacheLimit)
        {
            //cache = new ImageStreamCache(streamCacheLimit);
            cache = new BitmapImageCache(bmpCacheLimit, streamCacheLimit);
        }

        async Task<ImageSource> GetImage(string url)
        {
            return (ImageSource)await cache.Get(url);
        }

        public IObservable<ImageSource> GetImageAsync(string url)
        {
            return Observable.Create<ImageSource>(async observer =>
            {
                //try
                //{
                //    var imageStream = await cache.Get(url);
                //    // because you can't create BitmapImages on anything other than the 
                //    // UI thread, switch to UI before creating Bitmap and proccing OnNext
                //    GlobalDispatcher.Current.BeginInvoke(() =>
                //    {
                //        var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
                //        bmp.SetSource(imageStream);
                //        observer.OnNext((ImageSource)bmp);
                //        observer.OnCompleted();
                //    });
                //}
                //catch (Exception ex)
                //{
                //    GlobalDispatcher.Current.BeginInvoke(() =>
                //    {
                //        observer.OnError(ex);
                //    });
                //}


                try
                {

                    var image = await GetImage(url);
                    observer.OnNext(image);
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            });
        }

        public void Dispose()
        {
            cache.Dispose();
        }
    }
}
