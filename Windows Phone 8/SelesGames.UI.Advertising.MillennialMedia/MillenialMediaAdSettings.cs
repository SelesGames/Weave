using MillennialMedia.WP7.SDK;
using System.Windows;

namespace SelesGames.UI.Advertising.MillennialMedia
{
    public class MillennialMediaAdSettings : AdProviderSettingsBase
    {
        public string Apid { get; set; }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var adView = new MMBannerAdView
            {
                RefreshApid = Apid,
                RefreshTimer = DisplayTime,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };

            if (!string.IsNullOrEmpty(keywords))
            {
                var demo = new MMDemographic();
                demo["kw"] = keywords;
            }

            adView.ResumeRefreshTimer();

            return new MillennialMediaAdControlAdapter(adView);
        }
    }
}
