using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace Weave.LiveTile.ScheduledAgent
{
    public abstract class TileNegotiatorBase
    {
        protected ITileViewModel ViewModel { get; set; }
        ShellTile tile;
        string appName;

        public TileNegotiatorBase(string appName, ShellTile tile)
        {
            this.appName = appName;
            this.tile = tile;
        }

        protected abstract Task InitializeViewModelAsync();

        public async Task UpdateTile()
        {
            if (tile == null) return;

            await InitializeViewModelAsync();

            if (ViewModel == null) return;

            //ViewModel.AppName = appName;

            var newTileData = ViewModel.CreateTileData();
            tile.Update(newTileData);
        }

        public void UpdateLockScreen()
        {
            if (tile == null) return;
            if (ViewModel == null) return;

            ViewModel.UpdateLockScreen();
        }
    }
}
