using Common.Caching;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SelesGames.Common.Reactive
{
    public class ImageStreamCache : IDisposable
    {
        readonly LRUCache<string, Task<Stream>> cache;

        bool isDisposed = false;

        internal ImageStreamCache(int limit)
        {
            cache = new LRUCache<string, Task<Stream>>(limit);
        }

        internal Task<Stream> Get(string url)
        {
            EnsureNotDisposed();

            var existing = cache.Get(url);
            if (existing.IsSome)
            {
                return existing.Value;
            }
            else
            {
                var t = GetImageStreamAsync(url);
                var evicted = cache.AddOrUpdate(url, t);
                Dispose(evicted);
                return t;
            }
        }

        async Task<Stream> GetImageStreamAsync(string imageUrl)
        {
            var client = new System.Net.Http.HttpClient();
            var response = await client.GetAsync(imageUrl).ConfigureAwait(false);

            EnsureNotDisposed();
            response.EnsureSuccessStatusCode();

            var ms = new MemoryStream();
            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                stream.CopyTo(ms);
            }
            ms.Position = 0;
            return ms;
        }




        #region Dispose/Cleanup

        public async void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            var existingItems = cache.ToList();
            try
            {
                await Task.WhenAll(existingItems.Select(DisposeAsync));
                cache.Clear();
            }
            catch { }
        }

        async void Dispose(LRUCacheItem<string, Task<Stream>> evicted)
        {
            await DisposeAsync(evicted);
        }

        async Task DisposeAsync(LRUCacheItem<string, Task<Stream>> evicted)
        {
            if (evicted == null || evicted.Value == null)
                return;

            try
            {
                var stream = await evicted.Value;
                stream.Dispose();
            }
            catch { }
        }

        void EnsureNotDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(this.ToString());
        }

        #endregion
    }
}