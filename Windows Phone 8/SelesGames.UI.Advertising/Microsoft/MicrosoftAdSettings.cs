using System;
using System.Collections.Generic;

namespace SelesGames.UI.Advertising.Microsoft
{
    public class MicrosoftAdSettings
    {
        static Random r = new Random();

        public bool Enabled { get; set; }
        public string AppId { get; set; }
        public List<string> AdUnitIds { get; set; }

        public string GetRandomAdUnit()
        {
            if (AdUnitIds != null && AdUnitIds.Count > 0)
                return AdUnitIds[r.Next(0, AdUnitIds.Count)];
            else
                throw new Exception("No adunits have been set in SelesGames.UI.Advertising.Microsoft.MicrosoftAdSettings");
        }
    }
}
