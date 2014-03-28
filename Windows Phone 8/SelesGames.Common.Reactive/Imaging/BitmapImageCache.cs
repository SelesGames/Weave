using Common.Caching;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SelesGames.Common.Reactive
{
    public class BitmapImageCache : IDisposable
    {
        readonly ImageStreamCache streamCache;
        readonly LRUCache<string, Task<BitmapImage>> cache;

        bool isDisposed = false;

        public BitmapImageCache(int limit, int streamCacheLimit)
        {
            streamCache = new ImageStreamCache(streamCacheLimit);
            cache = new LRUCache<string, Task<BitmapImage>>(limit);
            cache.ItemEvicted += OnCacheItemEvicted;
        }

        public Task<BitmapImage> Get(string url)
        {
            EnsureNotDisposed();

            if (cache.ContainsKey(url))
            {
                return cache.Get(url);
            }
            else
            {
                var t = CreateBitmapAsync(url);
                cache.AddOrUpdate(url, t);
                return t;
            }
        }

        async Task<BitmapImage> CreateBitmapAsync(string url)
        {
            var stream = await streamCache.Get(url);

            EnsureNotDisposed();

            var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.IgnoreImageCache | BitmapCreateOptions.BackgroundCreation };
            bmp.SetSource(stream);
            return bmp;
        }

        void OnCacheItemEvicted(object sender, LRUEvictionEventArgs<string, Task<BitmapImage>> e)
        {
            if (e == null || e.Value == null)
                return;

            Dispose(e.Value);
        }




        #region Dispose/Cleanup

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            streamCache.Dispose();

            var existingItems = cache.ToList();
            foreach (var task in existingItems)
            {
                Dispose(task.Value);
            }
            cache.Clear();

            cache.ItemEvicted -= OnCacheItemEvicted;
        }

        void Dispose(Task<BitmapImage> task)
        {
            if (task == null)
                return;

            if (task.IsCompleted && task.Result != null)
            {
                var bmp = task.Result;
                bmp.UriSource = null;
            }
        }
   
        void EnsureNotDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(this.ToString());
        }

        #endregion
    }
}