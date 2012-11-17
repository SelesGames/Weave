using Microsoft.Phone.Controls;
using SelesGames.Phone;
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

            try
            {
                await publisherControl.LoadDataAsync();
            }
            catch { }

            DebugEx.WriteLine("page loaded");
            isPageLoaded = true;
        }

        void OnEmailLinkTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TaskService.ToEmailComposeTask(To: "info@selesgames.com");
        }
    }
}