using SelesGames.Rest;
using System;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising.Common
{
    public class AdSettingsClient
    {
        Lazy<Task<AdSettings>> initializer;
        string adSettingsUrl;

        public AdSettingsClient(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
            initializer = Lazy.Create(GetAdSettings);
        }

        async Task<AdSettings> GetAdSettings()
        {
            var client = new JsonRestClient<AdSettings>();
            var adSettings = await client.GetAsync(adSettingsUrl, System.Threading.CancellationToken.None);
            return adSettings;
        }
    }
}
