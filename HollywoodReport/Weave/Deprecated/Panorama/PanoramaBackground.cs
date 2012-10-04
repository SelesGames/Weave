using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using weave.Services;

namespace weave
{
    public partial class PanoramaBackground : UserControl
    {
        public PanoramaBackground()
        {
            Border border = new Border();
            border.CacheMode = new BitmapCache();
            border.SetBinding(Border.BackgroundProperty, new Binding("BackgroundBrush") { Source = PanoramicBackgroundManagerService.Current });
            this.Content = border;
        }
    }
}
