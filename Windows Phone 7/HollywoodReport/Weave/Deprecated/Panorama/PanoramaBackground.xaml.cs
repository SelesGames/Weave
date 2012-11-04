using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using weave.Services;

namespace weave
{
    public partial class PanoramaBackground : UserControl
    {
        public PanoramaBackground()
        {
            //InitializeComponent();
            Border border = new Border();
            border.CacheMode = new BitmapCache();
            border.SetBinding(Border.BackgroundProperty, new Binding("BackgroundBrush") { Source = PanoramicBackgroundManagerService.Current });
            this.Content = border;
        }
    }
}
