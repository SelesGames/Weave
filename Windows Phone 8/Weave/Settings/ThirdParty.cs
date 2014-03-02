using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.Settings
{
    public class ThirdParty
    {
        public PocketSettings Pocket { get; private set; }

        public ThirdParty()
        {
            Pocket = new PocketSettings();
        }
    }
}
