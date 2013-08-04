using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Weave.ViewModels;

namespace weave
{
    public class FeedsToCategoryOrLooseFeedViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<Feed>))
                return null;

            var result = new ObservableCollection<CategoryOrLooseFeedViewModel>();
            var feeds = (IEnumerable<Feed>)value;
            var sources = feeds.GetAllSources(o => o.ToUpper(), o => o).ToList();
            foreach (var source in sources)
                result.Add(source);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}