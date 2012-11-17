using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SelesGames.Rest;

namespace weave.UI.Advertising
{
    public class AdUnitsProviderService
    {
        //const string AD_UNITS_URL = "http://weavestorage.blob.core.windows.net/settings/adunits";

        string adUnitsUrl;

        public AdUnitsProviderService(string adUnitsUrl)
        {
            this.adUnitsUrl = adUnitsUrl;
        }

        public async Task<List<string>> GetAdUnitsAsync(CancellationToken cancelToken)
        {
            var client = new DelegateRestClient<string>(stream => stream.GetReadStream());
            var adUnitsText = await client.GetAsync(adUnitsUrl, cancelToken);

            using (var sr = new StringReader(adUnitsText))
            {
                var lines = sr.ReadLines().ToList();
                sr.Close();
                return lines;
            }
        }
    }
}