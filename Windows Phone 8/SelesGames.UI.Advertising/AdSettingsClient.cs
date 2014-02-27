﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising
{
    internal class AdSettingsClient
    {
        string adSettingsUrl;
        List<AdProviderSettingsBase> providerSettings;

        public AdSettingsClient(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
        }

        public async Task<AdSettings> Get()
        {
            var client = new SelesGames.HttpClient.SmartHttpClient();
            var response = await client.GetAsync(adSettingsUrl);

            if (!response.IsSuccessStatusCode)
                return new AdSettings { AreAdsActive = false };

            providerSettings = new List<AdProviderSettingsBase>();

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(responseString);

            CreateFromDynamic<Microsoft.MicrosoftAdSettings>(responseObject.Microsoft);
            CreateFromDynamic<AdDuplex.AdDuplexAdSettings>(responseObject.AdDuplex);

            var settings = new AdSettings
            {
                Providers = providerSettings
            };

            if (responseObject.AreAdsActive != null)
                settings.AreAdsActive = responseObject.AreAdsActive;

            return settings;
        }

        void CreateFromDynamic<T>(dynamic d)
        {
            if (d == null)
                return;

            try
            {
                var settings = JsonConvert.DeserializeObject<T>(d.ToString());
                providerSettings.Add(settings);
            }
            catch { }
        }
    }
}