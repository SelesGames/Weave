using System.Windows.Controls;

namespace weave
{
    public partial class MainPageNavigationDropDownList : UserControl
    {
        public MainPageNavigationDropDownList()
        {
            InitializeComponent();
        }

        void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
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
        }
    }
}