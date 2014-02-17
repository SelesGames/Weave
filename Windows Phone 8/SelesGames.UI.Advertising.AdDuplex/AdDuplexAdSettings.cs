using AdDuplex;

namespace SelesGames.UI.Advertising.AdDuplex
{
    public class AdDuplexAdSettings : AdProviderSettingsBase
    {
        public string AppId { get; set; }

        public override IAdControlAdapter CreateAdControl(string keywords)
        {
            var adControl = new AdControl
            {
                AppId = AppId,
                RefreshInterval = DisplayTime,
                Width = 480,
                Height = 80,
            };

            return new AdDuplexAdControlAdapter(adControl);
        }
    }
}
