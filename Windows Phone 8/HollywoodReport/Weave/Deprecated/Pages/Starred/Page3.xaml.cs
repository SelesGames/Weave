using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Phone.Controls;

namespace weave
{
    public partial class Page3 : PhoneApplicationPage
    {
        ObservableCollection<StarredNewsItem> obssource = new ObservableCollection<StarredNewsItem>();

        public static ImageCache imageCache = new ImageCache();

        public Page3()
        {
            InitializeComponent();
            StarredNewsItemsService.StarredNewsItemsStream.ObserveOnDispatcher()
                .Subscribe(o => SetListSource(o));
            lls.ItemsSource = obssource;
        }

        void SetListSource(IEnumerable<StarredNewsItem> source)
        {
            obssource.Clear();
            var set = source.ToList();
            foreach (var item in set)
            {
                obssource.Add(item);
            }
        }
    }
}