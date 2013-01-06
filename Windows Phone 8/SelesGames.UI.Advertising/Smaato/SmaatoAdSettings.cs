
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
                AdSpaceHeight = 80,
                AdSpaceWidth = 480,
                LocationUseOK = false,
                AdInterval = 30,
                Width = 480,
                Height = 80,
                ShowErrors = true,
            };

            if (!string.IsNullOrEmpty(keywords))
                adControl.Kws = keywords;

            adControl.StartAds();

            return new Smaato.SmaatoAdControlAdapter(adControl);
        }
    }
}
