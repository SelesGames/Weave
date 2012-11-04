using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;

namespace weave
{
    public partial class FacebookPost : PhoneApplicationPage
    {
        public FacebookPost()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.AppName.ToUpper();

            browser.Navigating += (s, e) =>
            {
                if (e.Uri.ToString().Contains("http://m.facebook.com/profile.php?posted_id="))
                {
                    MessageBox.Show("You successfully posted to your wall!");
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
            };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string url = null, text = "";
            if (NavigationContext.QueryString.ContainsKey("url") && NavigationContext.QueryString.ContainsKey("text"))
            {
                url = NavigationContext.QueryString["url"];
                text = NavigationContext.QueryString["text"];
            }
            if (!string.IsNullOrEmpty(url))
            {
                var temp = string.Format(@"http://m.facebook.com/sharer.php?u={0}&t={1}",
                                     HttpUtility.UrlEncode(url),
                                     HttpUtility.UrlEncode(text));
                var uri = temp.ToUri();
                if (uri != null)
                {
                    browser.Source = uri;
                    return;
                }
            }

            // this would be an error state - both uri and url were either null or malformed
            if (NavigationService.CanGoBack)
            {
                MessageBox.Show("Something is wrong with that article's link.");
                NavigationService.GoBack();
            }
        }
    }
}