using System.Collections.Generic;

namespace SelesGames.UI.Advertising
{
    internal class AdSettings
    {
        public bool AreAdsActive { get; set; }
        public IEnumerable<AdProviderSettingsBase> Providers { get; set; }

        public AdSettings()
        {
            AreAdsActive = true;
            Providers = new List<AdProviderSettingsBase>();
        }
    }
}