using System.Windows;
using System.Windows.Media;

namespace Weave.LiveTile.ScheduledAgent
{
    public class LiveTileViewModel
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
    }
}
