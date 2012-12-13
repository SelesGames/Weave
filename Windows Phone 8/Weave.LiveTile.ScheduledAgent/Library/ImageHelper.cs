using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Weave.LiveTile.ScheduledAgent
{
    internal static class ImageHelper
    {
        public static async Task<ImageSource> GetImageAsync(string url)
        {
            var bitmap = new WriteableBitmap(0, 0);

            var request = HttpWebRequest.CreateHttp(url);
            request.AllowReadStreamBuffering = true;
            var response = await request.GetResponseAsync();
            using (var stream = response.GetResponseStream())
            {
                bitmap.SetSource(stream);
                stream.Close();
            }
            return bitmap;
        }
    }
}
