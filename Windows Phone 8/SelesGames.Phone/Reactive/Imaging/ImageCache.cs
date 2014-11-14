using SelesGames.Common.Reactive;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Media;

namespace weave
{
    public class ImageCache : IDisposable
    {
        readonly BitmapImageCache cache;

        public ImageCache()
            : this(16, 32) { }

        public ImageCache(int bmpCacheLimit, int streamCacheLimit)
        {
            cache = new BitmapImageCache(bmpCacheLimit, streamCacheLimit);
        }

        public async Task<ImageSource> GetImage(string url)
        {
            return (ImageSource) await cache.Get(url);
        }

        public IObservable<ImageSource> GetImageAsync(string url)
        {
            return GetImage(url).ToObservable().ObserveOnDispatcher();
        }

        public void Dispose()
        {
            cache.Dispose();
        }
    }
}
