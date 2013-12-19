using SelesGames.Phone.Net.Http.Compression;
using System.Net.Http;

namespace SelesGames.Rest
{
    public class AutoCompressionHttpClient : HttpClient
    {
        public AutoCompressionHttpClient()
            : base(new HttpClientCompressionHandler())
        { }
    }
}
