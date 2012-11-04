using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using weave.Services.Twitter;

namespace weave
{
    public partial class TwitterAuthPage
    {
        TwitterRequestToken requestToken;

        public TwitterAuthPage()
        {
            InitializeComponent();
        }

        public void BeginOAuthWorkflow()
        {
            TwitterAuthService.GetRequestToken().ObserveOnDispatcher().Subscribe(
                token =>
                {
                    requestToken = token;
                    BrowserControl.Navigate(token.AuthorizeUrl);
                },
                exception => HandleRequestTokenError(exception));
        }

        void BrowserControl_Navigating(object sender, NavigatingEventArgs e)
        {
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;

            var callbackArgs = TwitterAuthService.GetCallbackArgs(e.Uri);
            if (callbackArgs != null)
            {
                e.Cancel = true;
                if (callbackArgs.DoTokensMatch(this.requestToken))
                {
                    TwitterAuthService.GetAccessToken(this.requestToken, callbackArgs).ObserveOnDispatcher().Subscribe(
                        accessToken =>
                        {
                            TwitterAccount.CurrentTwitterAccessCredentials = accessToken;
                            Close();
                        },
                        exception => HandleAccessTokenError(exception));
                }
                else
                    HandleTokensDontMatch();
            }
        }

        void Close()
        {
            Visibility = Visibility.Collapsed;
        }

        void BrowserControl_Navigated(object sender, NavigationEventArgs e)
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        void HandleRequestTokenError(Exception exception)
        {
            ;
        }

        void HandleAccessTokenError(Exception exception)
        {
            ;
        }

        void HandleTokensDontMatch()
        {
            ;
        }
    }
}