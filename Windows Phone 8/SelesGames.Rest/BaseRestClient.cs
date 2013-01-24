using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.Rest
{
    public class BaseRestClient
    {
        public Headers Headers { get; set; }
        public bool UseGzip { get; set; }

//        public Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken)
//        {
//#if NET40
//            var request = (HttpWebRequest)HttpWebRequest.Create(url);
//#else
//            var request = HttpWebRequest.CreateHttp(url);
//#endif
//            if (!string.IsNullOrEmpty(Headers.Accept))
//                request.Accept = Headers.Accept;

//            if (UseGzip)
//                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

//            return request
//                .GetResponseAsync()
//                .ContinueWith(
//                    task =>
//                    {
//                        var response = (HttpWebResponse)task.Result;
//                        if (response.StatusCode != HttpStatusCode.OK)
//                            throw new WebException(string.Format("Status code: {0}", response.StatusCode), null, WebExceptionStatus.UnknownError, response);


//                        var stream = response.GetResponseStream();
//                        var contentEncoding = response.Headers["Content-Encoding"];
//                        if (UseGzip || "gzip".Equals(contentEncoding, StringComparison.OrdinalIgnoreCase))
//        #if NET40
//                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
//        #else
//                            stream = new GZipInputStream(stream);
//        #endif
//                        return stream;
//                    },
//                    cancellationToken,
//                    TaskContinuationOptions.OnlyOnRanToCompletion,
//                    TaskScheduler.Default
//                );
//        }
    }
}
