using System;
using System.Net;
using System.Threading.Tasks;

namespace SelesGames.UrlShortener
{
    public class UrlShortenerService
    {
        public async Task<string> GetShortenedUrl(string url)
        {
            string tinyUrl = string.Format(
                "http://tinyurl.com/api-create.php?url={0}",
                Uri.EscapeDataString(url));

            var request = HttpWebRequest.Create(url);
            var response = await request.GetResponseAsync().ConfigureAwait(false);
            var result = await response.GetResponseStreamAsString().ConfigureAwait(false);
            return result;
        }
    }
}
