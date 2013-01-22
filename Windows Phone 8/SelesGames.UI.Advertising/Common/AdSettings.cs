using SelesGames.UI.Advertising.Inneractive;
using SelesGames.UI.Advertising.Microsoft;
using SelesGames.UI.Advertising.MobFox;
using SelesGames.UI.Advertising.Smaato;
using System.Collections.Generic;
using System.Linq;

namespace SelesGames.UI.Advertising.Common
{
    public class AdSettings
    {
        public MicrosoftAdSettings Microsoft { get; set; }
        public InneractiveAdSettings Inneractive { get; set; }
        public SmaatoAdSettings Smaato { get; set; }
        public MobFoxAdSettings MobFox { get; set; }
        public AdDuplexAdSettings AdDuplex { get; set; }

        public IEnumerable<AdSettingsBase> AsEnumerable()
        {
            Smaato.ExecutionOrder = -10;

            return new AdSettingsBase[] 
            {
                Microsoft,
                Inneractive,
                Smaato,
                MobFox,
                AdDuplex = new AdDuplexAdSettings { ExecutionOrder = 3, AppId = "7379" },
            }
            .OfType<AdSettingsBase>()
            .Where(o => o.Enabled)
            .OrderBy(o => o.ExecutionOrder)
            .ToList();
        }
    }
}
