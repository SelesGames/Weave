using System.Windows;
using Microsoft.Phone.Controls;

namespace weave
{
    public partial class InfoAndSupport : PhoneApplicationPage
    {
        public InfoAndSupport()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.Instance.AppName.ToUpper();
            buyButton.Content = "buy " + AppSettings.Instance.AppName.ToLower();
            versionInfoText.Text = AppSettings.Instance.AppName.ToLower() + " version " + AppSettings.Instance.VersionNumber;

            if (!AppSettings.Instance.IsTrial)
                buyButton.Visibility = Visibility.Collapsed;
        }

        void buyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelesGames.Phone.TaskService.ToMarketplaceDetailTask();
        }

        void rateOrReviewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelesGames.Phone.TaskService.ToMarketplaceReviewTask();
        }

        void emailUsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelesGames.Phone.TaskService.ToEmailComposeTask(
                To: "info@selesgames.com",
                Subject: string.Format("Question about {0} (version {1})", AppSettings.Instance.AppName, AppSettings.Instance.VersionNumber));
        }

        void followOnTwitterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelesGames.Phone.TaskService.ToInternetExplorerTask("http://www.twitter.com/SelesGames");
        }

        void ourOtherAppsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelesGames.Phone.TaskService.ToMarketplaceAppSearchTask("Seles Games");
        }
    }
}