using System.Net;
using System.Threading.Tasks;
using SelesGames.Rest;
using Weave.Readability;

namespace Weave.Mobilizer.Client
{
    public class Client
    {
        const string R_URL_TEMPLATE = "http://weave-mobilizer.cloudapp.net:8085/ipf?url={0}&f=json";

        public async Task<MobilizerResult> GetAsync(string url)
        {
            var client = new JsonRestClient<ReadabilityResult>();
            var encodedUrl = HttpUtility.UrlEncode(url);
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
