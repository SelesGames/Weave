using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SelesGames.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising.Common
{
    public class AdSettingsClient
    {
        Lazy<Task<IEnumerable<AdSettingsBase>>> initializer;
        string adSettingsUrl;
        IDictionary<string, Type> registeredProviders;

        public Task<IEnumerable<AdSettingsBase>> AdSettings { get { return initializer.Get(); } }

        public AdSettingsClient(string adSettingsUrl, bool loadDefault = true)
        {
            this.adSettingsUrl = adSettingsUrl;
            this.registeredProviders = new Dictionary<string, Type>();

            if (loadDefault)
            {
                Register("Microsoft", typeof(SelesGames.UI.Advertising.Microsoft.MicrosoftAdSettings));
                Register("AdDuplex", typeof(SelesGames.UI.Advertising.AdDuplex.AdDuplexAdSettings));
                Register("Smaato", typeof(SelesGames.UI.Advertising.Smaato.SmaatoAdSettings));
                Register("Inneractive", typeof(SelesGames.UI.Advertising.Inneractive.InneractiveAdSettings));
            }

            initializer = Lazy.Create(GetAdSettings);
        }

        public void Register(string propertyName, Type type)
        {
            registeredProviders.Add(propertyName, type);
        }

        async Task<IEnumerable<AdSettingsBase>> GetAdSettings()
        {
            List<AdSettingsBase> adSettingsVals = new List<AdSettingsBase>();

            var client = new AutoCompressionHttpClient();
            var stringResult = await client.GetStringAsync(adSettingsUrl);

            JObject jo = JObject.Parse(stringResult);
            foreach (var item in jo)
            {
                try
                {
                    var key = item.Key;
                    var stringRep = item.Value.ToString();
                    var type = registeredProviders[key];
                    var serialized = JsonConvert.DeserializeObject(stringRep, type);
                    var asb = (AdSettingsBase)serialized;
                    adSettingsVals.Add(asb);
                }
                catch { }
            }

            ///// REMOVE!!!
            //var set = adSettingsVals.OfType<Inneractive.InneractiveAdSettings>().Single();
            //set.Enabled = true;
            //adSettingsVals.Remove(set);
            //adSettingsVals.Insert(0, set);
            /////

            return adSettingsVals;
        }
    }
}