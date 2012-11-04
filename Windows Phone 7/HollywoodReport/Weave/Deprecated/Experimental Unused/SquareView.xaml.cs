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
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace weave
{
    public partial class SquareView : UserControl
    {
        public IEnumerable<NewsItem> News { get; set; }

        public SquareView()
        {
            InitializeComponent();
            //CountdownTimer.In(TimeSpan.FromSeconds(3)).Do(() =>
            //{
            //    listBox.ItemsSource = new VirtualAllNews();
            //});
            News = NewsItemService.GetAllNews().Take(40);
            DataContext = this;
        }

        void OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
