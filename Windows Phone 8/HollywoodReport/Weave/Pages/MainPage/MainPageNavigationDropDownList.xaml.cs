using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using SelesGames;

namespace weave
{
    public partial class MainPageNavigationDropDownList : UserControl, IDisposable, IPopup<CategoryOrLooseFeedViewModel>
    {
        ObservableCollection<CategoryOrLooseFeedViewModel> categoriesSource = new ObservableCollection<CategoryOrLooseFeedViewModel>();
        List<CategoryOrLooseFeedViewModel> lastSetOfSources = new List<CategoryOrLooseFeedViewModel>();
        CompositeDisposable disposables = new CompositeDisposable();

        public MainPageNavigationDropDownList()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            this.categories.ItemsSource = categoriesSource;
        }

        void categoryClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm  = (sender as Button).DataContext as CategoryOrLooseFeedViewModel;
            OnCategorySelected(vm);
        }

        void OnCategorySelected(CategoryOrLooseFeedViewModel catVM)
        {
            if (catVM.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category)
            {
                var category = catVM.Name;
                GlobalNavigationService.ToMainPage(category, "category");
            }
            else if (catVM.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed)
            {
                var feed = catVM.Name;
                GlobalNavigationService.ToMainPage(feed, catVM.FeedId);
            }

            BeginHide();
            //ResultCompleted.Raise(this, PopupResult.Create(catVM));
        }

        public async Task RefreshCategories()
        {
            scroller.ScrollToVerticalOffset(0d);

            var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
            var feeds = await dal.Feeds.Get();
            var sources = feeds.GetAllSources().ToList();

            if (lastSetOfSources.Except(sources).Any() || sources.Except(lastSetOfSources).Any())
            {
                lastSetOfSources = sources;
                categoriesSource.Clear();
                categoriesSource.SetSource(sources);
            }
        }

        public void HighlightCurrentCategory(string category)
        {
            if (this.categories.Items.Contains(category))
                this.categories.SelectedItem = category;
        }




        #region IPopup Methods

        public event EventHandler ShowCompleted;
        public event EventHandler HideCompleted;
        public event EventHandler<EventArgs<PopupResult<CategoryOrLooseFeedViewModel>>> ResultCompleted;

        public void BeginShow()
        {
            CloseSB.Stop();
            OpenSB.BeginWithNotification().Take(1).Subscribe(_ => ShowCompleted.Raise(this)).DisposeWith(disposables);
        }

        public void BeginHide()
        {
            CloseSB.BeginWithNotification().Take(1).Subscribe(_ => HideCompleted.Raise(this)).DisposeWith(disposables);
        }

        #endregion




        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
