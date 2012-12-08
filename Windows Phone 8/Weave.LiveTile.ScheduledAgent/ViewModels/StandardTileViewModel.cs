using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Media;

namespace Weave.LiveTile.ScheduledAgent.ViewModels
{
    public class StandardTileViewModel : ITileViewModel
    {
        public string AppName { get; set; }
        public string Category { get; set; }
        public ImageSource Source { get; set; }
        public string Headline { get; set; }
        public string NewCount { get; set; }
        public string ImageUrl { get; set; }

        public Visibility CategoryVisibility 
        { 
            get { return string.IsNullOrEmpty(Category) ? Visibility.Collapsed : Visibility.Visible; } 
        }

        public ShellTileData CreateTileData()
        {
            var frontTile = new LiveTileFront();
            var backTile = new LiveTileBack();

            frontTile.DataContext = this;
            backTile.DataContext = this;

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
