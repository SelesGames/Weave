using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace weave
{
    public partial class OAuthPage : PhoneApplicationPage
    {
        string target;

        public OAuthPage()
        {
            InitializeComponent();
            browser.IsScriptEnabled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            target = NavigationContext.QueryString["target"];
            browser.Navigate(new Uri(target));
            browser.Navigating += browser_Navigating;
        }

        void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            var url = e.Uri;
            //e.Cancel = true;
            System.Diagnostics.Debug.WriteLine(url);
        }
    }
}