using Microsoft.Phone.Shell;
using System;

namespace Weave.LiveTile.ScheduledAgent.ViewModels
{
    public class CycleTileViewModel : ITileViewModel
    {
        public string AppName { get; set; }

        public ShellTileData CreateTileData()
        {
            throw new NotImplementedException();
        }
    }
}
