using System;
using System.Collections.Generic;
using System.Linq;

namespace SelesGames.UI.Advertising
{
    internal class AdControlFactory
    {
        IEnumerator<AdProviderSettingsBase> adSettingsEnumerator;

        public AdControlFactory(IEnumerable<AdProviderSettingsBase> providers)
        {
            if (providers == null)
            {
                adSettingsEnumerator = new List<AdProviderSettingsBase>().GetEnumerator();
            }
            else
            {
                adSettingsEnumerator = providers
                    .Select(o => Tuple.Create(o, o.FaultToleranceCount))
                    .RepeatEnumerable()
                    .Wrap()
                    .GetEnumerator();
            }
        }

        public IAdControlAdapter CreateAdControl(string keywords = null)
        {
            if (adSettingsEnumerator == null || !adSettingsEnumerator.MoveNext())
                throw new InvalidOperationException("no adSettings have been set in AdControlFactory.CreateAdControl");

            var currentSelectedSetting = adSettingsEnumerator.Current;
            return currentSelectedSetting.CreateAdControl(keywords);
        }
    }
}
