using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising
{
    public class AdUnitCollection
    {
        string adUnitsUrl;
        List<string> adUnits = new List<string>();
        static Random r = new Random();
        Task areAdUnitsSet;

        public AdUnitCollection(string adUnitsUrl)
        {
            this.adUnitsUrl = adUnitsUrl;
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
                throw new Exception("No adunits have been set in SelesGames.UI.Advertising.AdUnitCollection");
        }

        async Task GetAndSetAdUnits()
        {
            try
            {
                var client = new AdUnitsProviderService(adUnitsUrl);
                var adUnits = await client.GetAdUnitsAsync(CancellationToken.None);
                this.adUnits.AddRange(adUnits);
            }
            catch { }
        }

        internal static AdUnitCollection Current { get; private set; }
    }
}
