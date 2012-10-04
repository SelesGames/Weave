using Microsoft.Phone.Controls;

namespace weave
{
    public partial class AddNewFeedPage : PhoneApplicationPage
    {
        public AddNewFeedPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.Instance.AppName.ToUpper();
        }

        void saveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //string feedName = this.feedName.Text;
            //string uri = this.uri.Text;

            //var newFeed = new FeedSource
            //{
            //    Category = "Custom",
            //    FeedName = feedName,
            //    FeedUri = uri,
            //    IsEnabled = true,
            //};

            //FeedsSettingsService.AddCustomFeed(newFeed);
            //NavigationService.GoBack();
        }

        void cancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            	//NavigationService.GoBack();
        }
    }
}
