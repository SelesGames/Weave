using SelesGames.Rest;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace SelesGames.UI.Advertising.Common
{
    public class AdSettingsClient
    {
        Lazy<Task<IEnumerable<AdSettingsBase>>> initializer;
        string adSettingsUrl;

        public Task<IEnumerable<AdSettingsBase>> AdSettings { get { return initializer.Get(); } }

        public AdSettingsClient(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
            initializer = Lazy.Create(GetAdSettings);
        }

        async Task<IEnumerable<AdSettingsBase>> GetAdSettings()
        {
            var registered = new[] 
            { 
                Tuple.Create("Microsoft", typeof(SelesGames.UI.Advertising.Microsoft.MicrosoftAdSettings)),
                Tuple.Create("AdDuplex", typeof(SelesGames.UI.Advertising.AdDuplex.AdDuplexAdSettings)),
                Tuple.Create("Smaato", typeof(SelesGames.UI.Advertising.Smaato.SmaatoAdSettings)),
                Tuple.Create("MobFox", typeof(SelesGames.UI.Advertising.MobFox.MobFoxAdSettings)),
                Tuple.Create("Inneractive", typeof(SelesGames.UI.Advertising.Inneractive.InneractiveAdSettings)),
            }.ToDictionary(o => o.Item1, o => o.Item2);

            List<AdSettingsBase> adSettingsVals = new List<AdSettingsBase>();

            var client = new RestStringClient();
            var stringResult = await client.GetAsync(adSettingsUrl, System.Threading.CancellationToken.None);

            JObject jo = JObject.Parse(stringResult);
            foreach (var item in jo)
            {
                var key = item.Key;
                var stringRep = item.Value.ToString();
                var type = registered[key];
                var serialized = JsonConvert.DeserializeObject(stringRep, type);
                var asb = (AdSettingsBase)serialized;
                adSettingsVals.Add(asb);
            }

            return adSettingsVals;



            //var client = new JsonRestClient();
            //var adSettings = await client.GetAsync<AdSettings>(adSettingsUrl, System.Threading.CancellationToken.None);
            //return adSettings;
        }
    }
}
