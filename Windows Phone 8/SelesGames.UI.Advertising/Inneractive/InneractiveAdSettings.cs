using Inneractive.Nokia.Ad;
using System.Collections.Generic;

namespace SelesGames.UI.Advertising.Inneractive
{
    internal class InneractiveAdSettings : AdProviderSettingsBase
    {
        public string AppId { get; set; }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var appId = AppId;

            var optionalParams = new Dictionary<InneractiveAd.IaOptionalParams, string>();
            if (!string.IsNullOrEmpty(keywords))
                optionalParams.Add(InneractiveAd.IaOptionalParams.Key_Keywords, keywords);

            optionalParams.Add(InneractiveAd.IaOptionalParams.Key_Ad_Alignment, InneractiveAd.IaAdAlignment.CENTER.ToString());
            optionalParams.Add(InneractiveAd.IaOptionalParams.Key_OptionalAdWidth, "480");
            optionalParams.Add(InneractiveAd.IaOptionalParams.Key_OptionalAdHeight, "80");

            var adControl = new InneractiveAd(appId, global::Inneractive.Ad.InneractiveAd.IaAdType.IaAdType_Banner, DisplayTime, optionalParams)
            {
                Width = 480,
                Height = 80,
            };

            return new InneractiveAdControlAdapter(adControl);
        }
    }
}
