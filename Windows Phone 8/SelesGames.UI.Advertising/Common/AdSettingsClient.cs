using SelesGames.Rest;
using System;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising.Common
{
    public class AdSettingsClient
    {
        Lazy<Task<AdSettings>> initializer;
        string adSettingsUrl;

        public Task<AdSettings> AdSettings { get { return initializer.Get(); } }

        public AdSettingsClient(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
            initializer = Lazy.Create(GetAdSettings);
        }

        async Task<AdSettings> GetAdSettings()
        {
            var client = new JsonRestClient();
            var adSettings = await client.GetAsync<AdSettings>(adSettingsUrl, System.Threading.CancellationToken.None);
            return adSettings;
        }
    }
}
