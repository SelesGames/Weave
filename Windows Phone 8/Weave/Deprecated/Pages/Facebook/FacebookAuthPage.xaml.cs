using System;
using System.Net;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using weave.Services.Facebook;

namespace weave
{
    public partial class FacebookAuthPage : PhoneApplicationPage
    {
        bool navigatedAway = false;
        bool isDirectionsPopupShowing = false;

        public FacebookAuthPage()
        {
            InitializeComponent();
            BrowserControl.IsScriptEnabled = true;
            CloseDirectionsPopup();

            this.GetLoaded().Take(1).Subscribe(notUsed =>
                BrowserControl.Navigate(FacebookAuthService.GetLoginPageUrl().ToUri()));
        }

        void BrowserControl_Navigating(object sender, NavigatingEventArgs e)
        {
            DebugEx.WriteLine(e.Uri.ToString());
            ProgressBar.Show();

            string strLoweredAddress = e.Uri.OriginalString.ToLower();

            string code = FacebookAuthService.GetCode(e.Uri);
            if (code != null)
            {
                e.Cancel = true;
                FacebookAuthService
                    .GetAccessTokenAsync(code)
                    .ObserveOnDispatcher()
                    .Subscribe(
                        token => SaveToken(token),
                        exception => HandleException());
            }

            else if (FacebookAuthService.GetIsAtPermissionsPage(e.Uri))
            {
                Observable.FromEventPattern<NavigationEventArgs>(BrowserControl, "Navigated").Take(1).Subscribe(o =>
                {
                    var html = BrowserControl.SaveToString();
                    if (FacebookAuthService.GetIsVerifiedAtPermissionsPage(html))
                        ShowDirectionsPopup();
                });
            }
        }

        void SaveToken(FacebookAccess accessToken)
        {
            ProgressBar.Hide();

            FacebookAccount.CurrentfacebookAccessCredentials = accessToken;
            if (!navigatedAway && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }      

        void HandleException()
        {
            ProgressBar.Hide();

            MessageBox.Show("There was a problem connecting to Facebook");
            if (!navigatedAway && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        void BrowserControl_Navigated(object sender, NavigationEventArgs e)
        {
            ProgressBar.Hide();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (isDirectionsPopupShowing)
            {
                CloseDirectionsPopup();
                e.Cancel = true;
                return;
            }
            base.OnBackKeyPress(e);
            navigatedAway = true;
        }

        void dismissButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseDirectionsPopup();
        }

        void ShowDirectionsPopup()
        {
            isDirectionsPopupShowing = true;
            directionsPopup.Visibility = Visibility.Visible;
        }

        void CloseDirectionsPopup()
        {
            isDirectionsPopupShowing = false;
            directionsPopup.Visibility = Visibility.Collapsed;
        }
    }
}