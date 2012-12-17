using Microsoft.Phone.Shell;

namespace Weave.LiveTile.ScheduledAgent.ViewModels
{
    public interface ITileViewModel
    {
        string AppName { get; set; }
        ShellTileData CreateTileData();
        void UpdateLockScreen();
    }
}
