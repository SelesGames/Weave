using Microsoft.Phone.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using Weave.ViewModels;

namespace weave
{
    public partial class ManageSourcesPage : PhoneApplicationPage
    {
        ManageSourcesViewModel viewModel;
        bool isBackLocked = false;

        public ManageSourcesPage()
        {
            InitializeComponent();
            viewModel = new ManageSourcesViewModel();
            DataContext = viewModel;

            lls.GotFocus += OnLLSGotFocus;
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition());
            SetValue(RadSlideContinuumAnimation.ApplicationHeaderElementProperty, this.PageTitle);
        }

        void OnLLSGotFocus(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            isBackLocked = true;
            await viewModel.LoadFeeds();
            isBackLocked = false;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (isBackLocked)
            {
                e.Cancel = true;
                return;
            }

            if (viewModel.AreThereTooManyFeeds)
            {
                e.Cancel = true;
                MessageBox.Show("You need to delete some sources before exiting this page", "TOO MANY SOURCES", MessageBoxButton.OK);
            }
        }




        #region Button and Touch Event Handling

        void FeedSettingsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null || element.Parent == null)
                return;

            var parentGrid = element.Parent;
            var contextMenu = ContextMenuService.GetContextMenu(parentGrid);
            if (contextMenu == null)
                return;

            if (contextMenu.Parent == null)
            {
                contextMenu.IsOpen = true;
            }
        }

        void FeedName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            var feed = element.DataContext as Feed;

            if (feed == null)
                return;

            NavigationService.ToEditSourcePage(feed);
        }

        async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var feed = menuItem.DataContext as Feed;
            var selectedOption = menuItem.Header.ToString();

            if (selectedOption.Equals("edit", StringComparison.OrdinalIgnoreCase))
                NavigationService.ToEditSourcePage(feed);

            else if (selectedOption.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await viewModel.DeleteSource(feed);
                }
                catch 
                { 
                    // do something here 
                }
            }
        }

        void AddFeedButton_Click(object sender, EventArgs e)
        {
            NavigationService.ToAddSourcePage();
        }

        #endregion
    }
}
