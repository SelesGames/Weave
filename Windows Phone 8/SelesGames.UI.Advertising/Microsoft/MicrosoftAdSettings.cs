using Microsoft.Advertising.Mobile.UI;
using Microsoft.Devices;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using MSD = Microsoft.Devices;

namespace SelesGames.UI.Advertising.Microsoft
{
    public class MicrosoftAdSettings : AdSettingsBase
    {
        static Random r = new Random();

        public string AppId { get; set; }
        public List<string> AdUnitIds { get; set; }

        string GetRandomAdUnit()
        {
            if (AdUnitIds != null && AdUnitIds.Count > 0)
                return AdUnitIds[r.Next(0, AdUnitIds.Count)];
            else
                throw new Exception("No adunits have been set in SelesGames.UI.Advertising.Microsoft.MicrosoftAdSettings");
        }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var appId = AppId;
            var adUnitId = GetRandomAdUnit();

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
                BorderBrush = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Black),
                Foreground = new SolidColorBrush(Colors.White),
            };

            if (!string.IsNullOrEmpty(keywords))
                adControl.Keywords = keywords;

            return new MicrosoftAdControlAdapter(adControl, DisplayTime);
        }
    }
}
