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

    //public class MainPageSourceListViewModel
    //{
    //    IUserCache userCache = ServiceResolver.Get<IUserCache>();

    //    public ObservableCollection<CategoryOrLooseFeedViewModel> Categories { get; private set; }

    //    public MainPageSourceListViewModel()
    //    {
    //        Categories = new ObservableCollection<CategoryOrLooseFeedViewModel>();
    //    }

    //    public void RefreshCategories()
    //    {
    //        var feeds = userCache.Get().Feeds;
    //        var sources = feeds.GetAllSources(o => o.ToUpper(), o => o).ToList();
    //        Merge(sources);
    //    }

    //    void Merge(List<CategoryOrLooseFeedViewModel> sources)
    //    {
    //        Categories.Clear();
    //        foreach (var source in sources)
    //            Categories.Add(source);
    //    }
    //}
}
