using SelesGames;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Weave.ViewModels.Contracts.Client;

namespace weave
{
    public class MainPageSourceListViewModel
    {
        IUserCache userCache = ServiceResolver.Get<IUserCache>();

        public ObservableCollection<CategoryOrLooseFeedViewModel> Categories { get; private set; }

        public MainPageSourceListViewModel()
        {
            Categories = new ObservableCollection<CategoryOrLooseFeedViewModel>();
        }

        public void RefreshCategories()
        {
            var feeds = userCache.Get().Feeds;
            var sources = feeds.GetAllSources(o => o.ToUpper(), o => o).ToList();
            Merge(sources);
        }

        void Merge(List<CategoryOrLooseFeedViewModel> sources)
        {
            Categories.Clear();
            foreach (var source in sources)
                Categories.Add(source);
        }
    }
}
