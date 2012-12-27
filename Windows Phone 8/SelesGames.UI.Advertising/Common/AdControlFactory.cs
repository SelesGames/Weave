using Inneractive.Nokia.Ad;
using Microsoft.Advertising.Mobile.UI;
using SelesGames.Rest;
using SelesGames.UI.Advertising.Inneractive;
using SelesGames.UI.Advertising.Microsoft;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SelesGames.UI.Advertising
{
    public class AdControlFactory
    {
        readonly string adSettingsUrl;// = "http://weave.blob.core.windows.net/settings/sampleSettings.json";
        Common.AdSettings adSettings;

        public AdControlFactory(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
        }

        async Task InitializeAsync()
        {
            if (adSettings == null)
            {
                var client = new JsonRestClient<Common.AdSettings>();
                adSettings = await client.GetAsync(adSettingsUrl, System.Threading.CancellationToken.None);
            }
        }

        public async Task<IAdControlAdapter> CreateAdControl(string keywords = null)
        {
            await InitializeAsync();
            return CreateMicrosoft(keywords);
            //return CreateInneractive(keywords);
        }

        IAdControlAdapter CreateMicrosoft(string keywords = null)
        {
            if (adSettings == null || adSettings.Microsoft == null) throw new ArgumentNullException("in AdControlFactory.CreateMicrosoft");

            var appId = adSettings.Microsoft.AppId;
            var adUnitId = adSettings.Microsoft.GetRandomAdUnit();

            if (string.IsNullOrEmpty(appId)) throw new ArgumentException("appId in AdControlFactory.CreateMicrosoft");
            if (string.IsNullOrEmpty(adUnitId)) throw new ArgumentException("adUnitId in AdControlFactory.CreateMicrosoft");

            var adControl = new AdControl(appId, adUnitId, false) { IsAutoCollapseEnabled = true, Width = 480, Height = 80, BorderBrush = new SolidColorBrush(Colors.White) };

            if (!string.IsNullOrEmpty(keywords))
                adControl.Keywords = keywords;

            return new MicrosoftAdControlAdapter(adControl);
        }

        IAdControlAdapter CreateInneractive(string keywords = null)
        {
            var adControl = new InneractiveAd("SelesGames_Weave_WP", InneractiveAd.IaAdType.IaAdType_Banner, 30)
            {
                Width = 480,
                Height = 80,
                BorderBrush = new SolidColorBrush(Colors.White)
            };

            if (!string.IsNullOrEmpty(keywords))
                adControl.Keywords = keywords;

            return new InneractiveAdControlAdapter(adControl);
        }
    }
}
