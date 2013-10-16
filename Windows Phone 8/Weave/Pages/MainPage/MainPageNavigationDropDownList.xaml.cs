using System;
using System.Windows.Controls;

namespace weave
{
    public partial class MainPageNavigationDropDownList : UserControl
    {
        public MainPageNavigationDropDownList()
        {
            InitializeComponent();
        }

        public event EventHandler<CategoryOrFeedEventArgs> ItemSelected;

        void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var vm  = (sender as Button).DataContext as NewsItemGroup;
            //OnCategorySelected(vm);
            if (ItemSelected != null && vm != null)
                ItemSelected(this, new CategoryOrFeedEventArgs(vm));
        }
        
        //void OnCategorySelected(CategoryOrLooseFeedViewModel catVM)
        //{
        //    if (catVM.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category)
        //    {
        //        var category = catVM.Name;

        //        GlobalNavigationService.ToMainPage(category, "category");
        //    }
        //    else if (catVM.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed)
        //    {
        //        var feed = catVM.Name;
        //        GlobalNavigationService.ToMainPage(feed, catVM.FeedId);
        //    }
        //}
    }

    public class CategoryOrFeedEventArgs : EventArgs
    {
        public NewsItemGroup Selected { get; private set; }

        public CategoryOrFeedEventArgs(NewsItemGroup selected)
        {
            Selected = selected;
        }
    }
}