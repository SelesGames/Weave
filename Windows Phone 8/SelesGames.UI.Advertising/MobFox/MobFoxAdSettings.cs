using MobFox.Ads.LocationAware;
using System;

namespace SelesGames.UI.Advertising.MobFox
{
    public class MobFoxAdSettings : AdSettingsBase
    {
        public string PublisherId { get; set; }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var publisherId = PublisherId;

            var adControl = new LocationAwareAdControl
            {
                PublisherID = publisherId,
                AlwaysShowAdsWhenDebuggerAttached = true,
                AutoRotate = true,
                Interval = TimeSpan.FromSeconds(30),
                ShowAdsOnlyInTrial = false,
                Width = 480,
                Height = 80,
            };

            return new MobFoxAdControlAdapter(adControl);
        }
    }
}
