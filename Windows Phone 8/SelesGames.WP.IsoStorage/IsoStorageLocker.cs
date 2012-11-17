using System;
using System.Threading;
using System.Threading.Tasks;


namespace SelesGames.WP.IsoStorage
{
    public class IsoStorageLocker<T> 
    {
        string key;
        IsoStorageClient<T> client;
        Func<T> generator;
        Lazy<Task<T>> holder;

        public bool IgnoreIsolatedStorage { get; set; }

        public IsoStorageLocker(string key, IsoStorageClient<T> client, Func<T> generator)
        {
            this.key = key;
            this.client = client;
            this.generator = generator;
            holder = Lazy.Create(GetFromIsoStorage);
        }

        async Task<T> GetFromIsoStorage()
        {
            T result;

            if (!IgnoreIsolatedStorage)
            {
                try
                {
                    result = await client.GetAsync(key, CancellationToken.None).ConfigureAwait(false);
                    return result;
                }
                catch { }
            }
            result = generator();
            return result;
        }

        public Task<T> Get()
        {
            return holder.Get();
        }

        public async Task Save()
        {
            var instance = await Get();
            await client.SaveAsync(key, instance, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
