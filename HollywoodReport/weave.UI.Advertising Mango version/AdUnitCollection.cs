using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace weave.UI.Advertising
{
    public class AdUnitCollection
    {
        string adUnitsUrl, defaultAdUnitId;
        List<string> adUnits = new List<string>();
        static Random r = new Random();
        Task areAdUnitsSet;

        public AdUnitCollection(string adUnitsUrl, string defaultAdUnitId)
        {
            this.adUnitsUrl = adUnitsUrl;
            this.defaultAdUnitId = defaultAdUnitId;
            Current = this;
        }

        public Task AreAdUnitsSet 
        { 
            get
            {
                if (areAdUnitsSet == null)
                {
                    areAdUnitsSet = GetAndSetAdUnits();
                }
                return areAdUnitsSet;
            }
        } 

        public string GetRandomAdUnit()
        {
            if (adUnits.Count > 0)
                return adUnits[r.Next(0, adUnits.Count)];
            else
                return defaultAdUnitId;
        }

        async Task GetAndSetAdUnits()
        {
            try
            {
                var client = new AdUnitsProviderService(adUnitsUrl);
                var adUnits = await client.GetAdUnitsAsync(CancellationToken.None);
                this.adUnits.AddRange(adUnits);
            }
            catch { this.adUnits.Add(defaultAdUnitId); }
        }

        internal static AdUnitCollection Current { get; private set; }
    }
}
