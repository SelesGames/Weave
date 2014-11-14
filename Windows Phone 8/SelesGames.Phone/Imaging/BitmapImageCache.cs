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
        }

        internal Task<BitmapImage> Get(string url)
        {
            EnsureNotDisposed();

            var existing = cache.Get(url);
            if (existing.IsSome)
            {
                return existing.Value;
            }
            else
            {
                var t = CreateBitmapAsync(url);
                var evicted = cache.AddOrUpdate(url, t);
                Dispose(evicted);
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




        #region Dispose/Cleanup

        //public void Dispose()
        //{
        //    if (isDisposed)
        //        return;

        //    isDisposed = true;

        //    streamCache.Dispose();

        //    var existingItems = cache.ToList();
        //    foreach (var task in existingItems)
        //    {
        //        Dispose(task.Value);
        //    }
        //    cache.Clear();
        //}

        public async void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            streamCache.Dispose();

            var existingItems = cache.ToList();
            try
            {
                await Task.WhenAll(existingItems.Select(DisposeAsync));
                cache.Clear();
            }
            catch { }
        }

        async void Dispose(LRUCacheItem<string, Task<BitmapImage>> evicted)
        {
            await DisposeAsync(evicted);
        }

        async Task DisposeAsync(LRUCacheItem<string, Task<BitmapImage>> evicted)
        {
            if (evicted == null || evicted.Value == null)
                return;

            try
            {
                var bmp = await evicted.Value;
                bmp.UriSource = null;
            }
            catch { }
        }

        //async void Dispose(Task<BitmapImage> task)
        //{
        //    if (task == null)
        //        return;

        //    try
        //    {
        //        var bmp = await task;
        //        bmp.UriSource = null;
        //    }
        //    catch { }
        //    //if (task.IsCompleted && task.Result != null)
        //    //{
        //    //    var bmp = task.Result;
        //    //    bmp.UriSource = null;
        //    //}
        //}
   
        void EnsureNotDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(this.ToString());
        }

        #endregion
    }
}