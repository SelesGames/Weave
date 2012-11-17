using Microsoft.Phone.Shell;
using System;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class LiveTileViewModelExtensions
    {
        public static StandardTileData CreateTileData(this LiveTileViewModel viewModel)
        {
            var frontTile = new LiveTileFront();
            var backTile = new LiveTileBack();

            frontTile.DataContext = viewModel;
            backTile.DataContext = viewModel;

            var frontUri = frontTile.GetLiveTileUri("front" + Guid.NewGuid().ToString());
            var backUri = backTile.GetLiveTileUri("back" + Guid.NewGuid().ToString());

            var newTileData = new StandardTileData
            {
                BackgroundImage = frontUri,
                BackBackgroundImage = backUri,
                BackContent = string.Empty,
                BackTitle = string.Empty,
                Count = null,
                Title = string.Empty,
            };

            return newTileData;
        }
    }
}
