using Microsoft.Phone.Controls;
using SelesGames;
using SelesGames.Phone;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Weave.UI.Frame;

namespace weave.Pages.Settings
{
    public partial class BrowseFeedsByCategoryPage : PhoneApplicationPage
    {
        BrowseFeedsByCategoryViewModel viewModel;

        public BrowseFeedsByCategoryPage()
        {
            InitializeComponent();
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition());
            SetValue(RadSlideContinuumAnimation.ApplicationHeaderElementProperty, this.PageTitle);
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (viewModel != null)
                return;

            string category;
            if (NavigationContext.QueryString.TryGetValue("category", out category))
            {
                viewModel = new BrowseFeedsByCategoryViewModel(category);
                DataContext = viewModel;
                await Task.Yield();
                await viewModel.LoadFeedsAsync();
            }
        }

        protected async override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            var t = viewModel.SaveChanges();
            if (t.IsCompleted)
                return;
            else
            {
                e.Cancel = true;

                var frame = ServiceResolver.Get<OverlayFrame>();
                frame.OverlayText = "Saving...";
                frame.IsLoading = true;

                await t;

                frame.IsLoading = false;

                NavigationService.TryGoBack();
            }
        }
    }
}