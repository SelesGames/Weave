
namespace SelesGames.UI.Advertising.Smaato
{
    public class AdDuplexAdSettings : AdSettingsBase
    {
        public string AppId { get; set; }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var adControl = new AdDuplex.AdControl
            {
                AppId = AppId,
                RefreshInterval = 30,
                Width = 480,
                Height = 80,
            };

            return new AdDuplexAdControlAdapter(adControl);
        }
    }
}
