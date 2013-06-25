using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;
using System.Net;
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
            var request = HttpWebRequest.CreateHttp(url);
            return request.GetResponseAsync();
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            var request = HttpWebRequest.CreateHttp(url);

            if (!string.IsNullOrEmpty(Headers.Accept))
                request.Accept = Headers.Accept;

            if (UseGzip)
                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException webException)
            {
                response = (HttpWebResponse)webException.Response;
                throw webException;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (response.StatusCode == HttpStatusCode.OK)
                return ReadObjectFromWebResponse<T>(response);
            //else
            //    return default(T);
                        
            throw new WebException(string.Format("Status code: {0}", response.StatusCode), null, WebExceptionStatus.UnknownError, response);
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancelToken)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.Method = "POST";

            if (!string.IsNullOrEmpty(Headers.ContentType))
                request.ContentType = Headers.ContentType;

            if (!string.IsNullOrEmpty(Headers.Accept))
                request.Accept = Headers.Accept;

            if (UseGzip)
                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                cancelToken.ThrowIfCancellationRequested();
                WriteObject(requestStream, obj);
                requestStream.Close();
            }

            var response = await request.GetResponseAsync().ConfigureAwait(false);

            cancelToken.ThrowIfCancellationRequested();

            return ReadObjectFromWebResponse<TResult>((HttpWebResponse)response);
        }

        public async Task<bool> PostAsync<TPost>(string url, TPost obj, CancellationToken cancelToken)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.Method = "POST";

            if (!string.IsNullOrEmpty(Headers.ContentType))
                request.ContentType = Headers.ContentType;

            if (!string.IsNullOrEmpty(Headers.Accept))
                request.Accept = Headers.Accept;

            if (UseGzip)
                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                cancelToken.ThrowIfCancellationRequested();
                WriteObject(requestStream, obj);
                requestStream.Close();
            }

            var response = await request.GetResponseAsync().ConfigureAwait(false);
            var httpResponse = (HttpWebResponse)response;
            return httpResponse.StatusCode == HttpStatusCode.Created;
        }




        #region helper methods

        T ReadObjectFromWebResponse<T>(HttpWebResponse response)
        {
            T result;

            using (var stream = response.GetResponseStream())
            {
                var contentEncoding = response.Headers["Content-Encoding"];
                if (UseGzip || "gzip".Equals(contentEncoding, StringComparison.OrdinalIgnoreCase))
                {
#if NET40
                    using (var gzip = new GZipStream(stream, CompressionMode.Decompress, false))
#else
                    using (var gzip = new GZipInputStream(stream))
#endif
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
