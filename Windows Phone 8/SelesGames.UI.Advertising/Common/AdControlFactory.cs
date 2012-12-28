using Inneractive.Nokia.Ad;
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Devices;
using SelesGames.Rest;
using SelesGames.UI.Advertising.Inneractive;
using SelesGames.UI.Advertising.Microsoft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using MSD = Microsoft.Devices;

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

            if (MSD.Environment.DeviceType == DeviceType.Emulator)
            {
                appId = "test_client";
                adUnitId = "Image480_80";
            }

            if (string.IsNullOrEmpty(appId)) throw new ArgumentException("appId in AdControlFactory.CreateMicrosoft");
            if (string.IsNullOrEmpty(adUnitId)) throw new ArgumentException("adUnitId in AdControlFactory.CreateMicrosoft");

            var adControl = new AdControl(appId, adUnitId, false) 
            { 
                IsAutoCollapseEnabled = true, 
                Width = 480, 
                Height = 80, 
                BorderBrush = null//new SolidColorBrush(Colors.White) 
            };

            if (!string.IsNullOrEmpty(keywords))
                adControl.Keywords = keywords;

            return new MicrosoftAdControlAdapter(adControl);
        }

        IAdControlAdapter CreateInneractive(string keywords = null)
        {
            if (adSettings == null || adSettings.Inneractive == null) throw new ArgumentNullException("in AdControlFactory.CreateMicrosoft");

            var appId = adSettings.Inneractive.AppId;

            var grid = new Grid
            {
                Width = 480,
                Height = 80,
            };

            var optionalParams = new Dictionary<InneractiveAd.IaOptionalParams,string>();
            if (!string.IsNullOrEmpty(keywords))
                optionalParams.Add(InneractiveAd.IaOptionalParams.Key_Keywords, keywords);

            InneractiveAd.DisplayAd(appId, global::Inneractive.Ad.InneractiveAd.IaAdType.IaAdType_Banner, grid, 30, optionalParams);

            //var adControl = new InneractiveAd("SelesGames_Weave_WP", InneractiveAd.IaAdType.IaAdType_Banner, 30)
            //{
            //    Width = 480,
            //    Height = 80,
            //    BorderBrush = new SolidColorBrush(Colors.White)
            //};


            return new InneractiveAdControlAdapter(grid);
        }
    }
}
