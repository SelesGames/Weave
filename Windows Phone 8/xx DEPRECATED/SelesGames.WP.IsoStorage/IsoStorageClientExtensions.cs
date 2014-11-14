using System;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.IsoStorage
{
    public static class IsoStorageClientExtensions
    {
        public async static Task<T> GetOrDefaultAsync<T>(
            this IsoStorageClient<T> client,
            string fileName,
            CancellationToken cancellationToken,
            Func<T> generator = null)
        {
            try
            {
                var result = await client.GetAsync(fileName, cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch { }

            if (generator == null)
                return default(T);
            else
            {
                return generator();
            }
        }

        public static Task<T> GetOrDefaultAsync<T>(
            this IsoStorageClient<T> client,
            string fileName,
            Func<T> generator = null)
        {
            return GetOrDefaultAsync(client, fileName, CancellationToken.None, generator);
        }

        public static Task SaveAsync<T>(this IsoStorageClient<T> client, string fileName, T obj)
        {
            return client.SaveAsync(fileName, obj, CancellationToken.None);
        }
    }
}
