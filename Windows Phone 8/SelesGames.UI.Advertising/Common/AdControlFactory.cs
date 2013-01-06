using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising
{
    public class AdControlFactory
    {
        Common.AdSettingsClient client;
        IEnumerator<AdSettingsBase> adSettingsEnumerator;

        public AdControlFactory(Common.AdSettingsClient client)
        {
            this.client = client;
        }

        async Task InitializeAsync()
        {
            if (adSettingsEnumerator == null)
            {
                var adSettings = await client.AdSettings;

                adSettingsEnumerator = adSettings
                    .AsEnumerable()
                    .Select(o => Tuple.Create(o, o.FaultToleranceCount))
                    .RepeatEnumerable()
                    .Wrap()
                    .GetEnumerator(); 
            }
        }

        public async Task<IAdControlAdapter> CreateAdControl(string keywords = null)
        {
            await InitializeAsync();

            if (adSettingsEnumerator == null || !adSettingsEnumerator.MoveNext())
                throw new InvalidOperationException("no adSettings have been set in AdControlFactory.CreateAdControl");

            var currentSelectedSetting = adSettingsEnumerator.Current;
            return currentSelectedSetting.CreateAdControl(keywords);
        }
    }
}
