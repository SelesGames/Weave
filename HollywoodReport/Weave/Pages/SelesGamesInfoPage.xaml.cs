using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;

namespace weave
{
    public partial class SelesGamesInfoPage : PhoneApplicationPage
    {
        bool isPageLoaded = false;

        public SelesGamesInfoPage()
        {
            InitializeComponent();
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumTransition());
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (isPageLoaded)
                return;

            publisherControl.PublisherName = "Seles Games";
            await publisherControl.LoadDataAsync();
            DebugEx.WriteLine("page loaded");
            isPageLoaded = true;
        }
    }
}