using SelesGames.Rest;
using System.Net;
using System.Threading.Tasks;
using Weave.Readability;

namespace Weave.Mobilizer.Client
{
    public class Client
    {
        const string R_URL_TEMPLATE = "http://mobilizer.cloudapp.net/ipf?url={0}";

        public async Task<MobilizerResult> GetAsync(string url)
        {
            var client = new JsonRestClient<ReadabilityResult>();
            var encodedUrl = WebUtility.UrlEncode(url);
            var fUrl = string.Format(R_URL_TEMPLATE, encodedUrl);
            var result = await client.GetAsync(fUrl, System.Threading.CancellationToken.None).ConfigureAwait(false);
            return Parse(result);            
        }

        MobilizerResult Parse(ReadabilityResult result)
        {
            return new MobilizerResult
            {
                author = result.author,
                content = result.content,
                date_published = result.date_published,
                domain = result.domain,
                title = result.title,
                url = result.url,
                word_count = result.word_count,
            };
        }
    }
}
