using Common.Caching;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SelesGames.Common.Reactive
{
    public class ImageStreamCache : IDisposable
    {
        readonly LRUCache<string, Task<Stream>> cache;

        bool isDisposed = false;

        public ImageStreamCache(int limit)
        {
            cache = new LRUCache<string, Task<Stream>>(limit);
            cache.ItemEvicted += OnCacheItemEvicted;
        }

        public Task<Stream> Get(string url)
        {
            EnsureNotDisposed();

            if (cache.ContainsKey(url))
            {
                return cache[url];
            }
            else
            {
                var t = GetImageStreamAsync(url);
                cache[url] = t;
                return t;
            }
        }

        async Task<Stream> GetImageStreamAsync(string imageUrl)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(imageUrl).ConfigureAwait(false);

            EnsureNotDisposed();
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return stream;
        }

        void OnCacheItemEvicted(object sender, LRUEvictionEventArgs<string, Task<Stream>> e)
        {
            if (e == null || e.Value == null)
                return;

            Task.Run(() =>
            {
                //DebugEx.WriteLine("cleaning image stream: {0}", e.Key);
                Dispose(e.Value);
            });
        }




        #region Dispose/Cleanup

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            var existingItems = cache.ToList();
            foreach (var task in existingItems)
            {
                Dispose(task.Value);
            }
            cache.Clear();

            cache.ItemEvicted -= OnCacheItemEvicted;
        }

        void Dispose(Task<Stream> task)
        {
            if (task == null)
                return;

            if (task.IsCompleted && task.Result != null)
            {
                var stream = task.Result;
                stream.Dispose();
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