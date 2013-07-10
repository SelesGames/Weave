using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.Rest
{
    public abstract class RestClient : BaseRestClient
    {
        public RestClient()
        {
            Headers = new Headers();
        }

        public Task GetAsync(string url, CancellationToken cancellationToken)
        {
            return new HttpClient().GetAsync(url, cancellationToken);
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            var response = await CreateClient().GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await ReadObjectFromResponseMessage<T>(response);
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancelToken)
        {
            var client = CreateClient();

            HttpResponseMessage response;

            using (var ms = new MemoryStream())
            {
                WriteObject(ms, obj);
                ms.Position = 0;

                var content = new StreamContent(ms);
                content.Headers.TryAddWithoutValidation("Content-Type", Headers.ContentType);

                response = await client.PostAsync(url, content).ConfigureAwait(false);
                ms.Close();
            }

            response.EnsureSuccessStatusCode();
            return await ReadObjectFromResponseMessage<TResult>(response);
        }

        public async Task PostAsync<TPost>(string url, TPost obj, CancellationToken cancelToken)
        {
            var client = CreateClient();

            using (var ms = new MemoryStream())
            {
                WriteObject(ms, obj);
                ms.Position = 0;

                var content = new StreamContent(ms);
                content.Headers.TryAddWithoutValidation("Content-Type", Headers.ContentType);

                await client.PostAsync(url, content).ConfigureAwait(false);
                ms.Close();
            }
        }




        #region helper methods

        HttpClient CreateClient()
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(Headers.Accept))
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", Headers.Accept);

            if (UseGzip)
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip");

            return client;
        }

        async Task<T> ReadObjectFromResponseMessage<T>(HttpResponseMessage response)
        {
            T result;

            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                var contentEncoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
                if (UseGzip || "gzip".Equals(contentEncoding, StringComparison.OrdinalIgnoreCase))
                {
                    using (var gzip = new GZipInputStream(stream))
                    {
                        result = ReadObject<T>(gzip);
                        gzip.Close();
                    }
                }
                else
                {
                    result = ReadObject<T>(stream);
                }
                stream.Close();
            }
            return result;
        }

        #endregion




        protected abstract T ReadObject<T>(Stream readStream);
        protected abstract void WriteObject<T>(Stream writeStream, T obj);
    }
}
