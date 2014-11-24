using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising
{
    internal class AdSettingsClient
    {
        string adSettingsUrl;
        List<AdProviderSettingsBase> providerSettings;
        Formatting f = new Formatting();

        public AdSettingsClient(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
        }

        public async Task<AdSettings> Get()
        {
            var client = new SmartHttpClient(compressionSettings: CompressionSettings.None);
            var response = await client.GetAsync(adSettingsUrl);

            if (!response.HttpResponseMessage.IsSuccessStatusCode)
                return new AdSettings { AreAdsActive = false };

            providerSettings = new List<AdProviderSettingsBase>();

            var responseString = await response.HttpResponseMessage.Content.ReadAsStringAsync();
            JObject o = JObject.Parse(responseString);
            
            CreateFromToken<Microsoft.MicrosoftAdSettings>(o["Microsoft"]);
            CreateFromToken<AdDuplex.AdDuplexAdSettings>(o["AdDuplex"]);

            var settings = new AdSettings
            {
                Providers = providerSettings
            };

            var adsActiveToken = o["AreAdsActive"];
            if (adsActiveToken != null)
                settings.AreAdsActive = (bool)adsActiveToken;// adsActiveToken.Value<bool responseObject.AreAdsActive;

            return settings;
        }

        void CreateFromToken<T>(JToken token)
            where T : AdProviderSettingsBase
        {
            if (token == null)
                return;

            var stringRep = token.ToString(f);
            CreateFromString<T>(stringRep);
        }

        void CreateFromString<T>(string d)
            where T : AdProviderSettingsBase
        {
            if (string.IsNullOrWhiteSpace(d))
                return;

            try
            {
                var settings = JsonConvert.DeserializeObject<T>(d);
                providerSettings.Add(settings);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }
    }
}