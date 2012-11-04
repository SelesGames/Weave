using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace weave
{
    public partial class TwitterPost : PhoneApplicationPage
    {
        public TwitterPost()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.AppName.ToUpper();

            browser.Navigating += (s, e) =>
            {
                if (e.Uri.ToString().Contains("http://m.twitter.com/share/complete"))
                {
                    MessageBox.Show("You successfully tweeted!");
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
                var temp = string.Format(@"http://m.twitter.com/share?url={0}&text={1}",
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