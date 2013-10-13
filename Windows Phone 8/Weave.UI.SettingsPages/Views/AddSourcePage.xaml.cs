using Microsoft.Phone.Controls;
using SelesGames.Phone;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace weave.Pages.Settings
{
    public partial class AddSourcePage : PhoneApplicationPage
    {
        AddSourceViewModel viewModel;
        bool areCategoriesLoaded = false;
        string lastSearchString = string.Empty;

        public AddSourcePage()
        {
            InitializeComponent();
            busyIndicator.CollapseAndStopAnimation();
            viewModel = new AddSourceViewModel();
            DataContext = viewModel;
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumTransition());
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string section = null;

            if (NavigationContext.QueryString.ContainsKey("section"))
            {
                section = NavigationContext.QueryString["section"];
            }

            if (section == null || section == "browse")
                pivot.SelectedIndex = 0;
            else if (section == "search")
                pivot.SelectedIndex = 1;


            if (areCategoriesLoaded)
                return;

            try
            {
                await viewModel.LoadCategories();
                areCategoriesLoaded = true;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Error downloading categories");
            }
        }




        #region Browse Category

        void OnCategoryTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var category = ((Button)sender).DataContext as weave.AddSourceViewModel.Category;
            SetValue(RadContinuumAnimation.ContinuumElementProperty, sender);
            NavigationService.ToBrowseFeedsByCategoryPage(category.Name);
        }

        #endregion




        #region Search

        void searchText_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoSearch();
                this.Focus();
            }
        }

        async Task DoSearch()
        {
            var searchString = searchText.Text;
            if (string.IsNullOrEmpty(searchString) || viewModel == null || lastSearchString == searchString)
                return;

            lastSearchString = searchString;
            viewModel.SearchString = searchString;

            busyIndicator.MakeVisibleAndStartAnimation();

            await viewModel.LoadSearchResults();

            busyIndicator.CollapseAndStopAnimation();
        }

        async void OnSearchToggleTapped(object sender, System.Windows.RoutedEventArgs e)
        {
            var source = (AddSourceViewModel.Source)((FrameworkElement)sender).DataContext;
            if (!source.IsAdded)
                await viewModel.AddFeed(source);
            else
                await viewModel.RemoveFeed(source);
        }

        async void OnSearchResultTextTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var source = (AddSourceViewModel.Source)((FrameworkElement)sender).DataContext;
            if (!source.IsAdded)
            {
                await viewModel.AddFeed(source);
            }
            else if (source.IsAdded && source.Feed != null)
            {
                SetValue(RadContinuumAnimation.ContinuumElementProperty, sender);
                NavigationService.ToEditSourcePage(source.Feed);
            }
        }

        #endregion




        async Task SaveAllNewFeeds()
        {
            //await viewModel.SaveGReaderFeeds();
        }

        protected async override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            var t = SaveAllNewFeeds();
            if (t.IsCompleted)
                return;
            else
            {
                e.Cancel = true;
                await t;
                NavigationService.TryGoBack();
            }
        }
    }
}