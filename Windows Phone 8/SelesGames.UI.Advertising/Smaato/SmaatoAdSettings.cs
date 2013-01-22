using System.Windows;

namespace SelesGames.UI.Advertising.Smaato
{
    public class SmaatoAdSettings : AdSettingsBase
    {
        public int PublisherId { get; set; }
        public int AdspaceId { get; set; }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var publisherId = PublisherId;
            var adspaceId = AdspaceId;

            var adControl = new SOMAWP8.SomaAdViewer
            {
                Pub = publisherId,
                Adspace = adspaceId,
                //AdSpaceHeight = 80,  // DONT SET THIS
                //AdSpaceWidth = 480,  // DONT SET THIS
                LocationUseOK = true,
                AdInterval = 30,
                //Width = 478,
                //Height = 78,
                ShowErrors = false,  // SET TO FALSE ALWAYS FOR PRODUCTION
                PopupAd = false,     // SET TO FALSE ALWAYS - HIDES ADS AFTER 10 SECONDS
                PopupAdDuration = 30,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };

            if (!string.IsNullOrEmpty(keywords))
            {
                adControl.Kws = keywords;
                //adControl.Qs = keywords;
            }

            adControl.StartAds();

            return new Smaato.SmaatoAdControlAdapter(adControl);
        }
    }
}
