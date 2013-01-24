using Inneractive.Nokia.Ad;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SelesGames.UI.Advertising.Inneractive
{
    public class InneractiveAdSettings : AdSettingsBase
    {
        public string AppId { get; set; }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var appId = AppId;

            var grid = new Grid
            {
                Width = 480,
                Height = 80,
            };

            var optionalParams = new Dictionary<InneractiveAd.IaOptionalParams, string>();
            if (!string.IsNullOrEmpty(keywords))
                optionalParams.Add(InneractiveAd.IaOptionalParams.Key_Keywords, keywords);

            InneractiveAd.DisplayAd(appId, global::Inneractive.Ad.InneractiveAd.IaAdType.IaAdType_Banner, grid, DisplayTime, optionalParams);

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
