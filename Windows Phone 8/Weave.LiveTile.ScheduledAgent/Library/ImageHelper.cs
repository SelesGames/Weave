using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Weave.LiveTile.ScheduledAgent
{
    internal static class ImageHelper
    {
        //public static async Task<ImageSource> GetImageAsync(string url)
        //{
        //    var bitmap = new WriteableBitmap(0, 0);

        //    var request = HttpWebRequest.CreateHttp(url);
        //    request.AllowReadStreamBuffering = true;
        //    var response = await request.GetResponseAsync();
        //    using (var stream = response.GetResponseStream())
        //    {
        //        bitmap.SetSource(stream);
        //        stream.Close();
        //    }
        //    return bitmap;
        //}

        public static Task<Stream> GetImageStreamAsync(string url)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.AllowReadStreamBuffering = true;
            return request.GetResponseAsync().ContinueWith(response =>
            {
                var httpResponse = (HttpWebResponse)response.Result;
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                    return httpResponse.GetResponseStream();
                else
                    throw new WebException();
            }, 
            TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
