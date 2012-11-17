using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using weave.Services.Twitter;

namespace weave
{
    public partial class TweetPage
    {
        bool navigatedAway = false;

        public TweetPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.AppName.ToUpper();
            TweetTextBox.Text = string.Empty;
            TweetTextBox.IsEnabled = false;
            shorteningTextBlurb.Visibility = Visibility.Collapsed;

            if (TwitterAccount.CurrentTwitterAccessCredentials == null)
            {
                authPage.Visibility = Visibility.Visible;
                authPage.BeginOAuthWorkflow();
            }

            Observable.FromEventPattern<OrientationChangedEventArgs>(GlobalNavigationService.CurrentFrame, "OrientationChanged")
                .Select(o => o.EventArgs)
                .Subscribe(e =>
                {
                    if (e.Orientation.IsAnyLandscape())
                        birdButton.Visibility = Visibility.Collapsed;
                    else if (e.Orientation.IsAnyPortrait())
                        birdButton.Visibility = Visibility.Visible;
                });

            UpdateRemainingCharacters();

            birdButton.GetClick().Merge(tweetButton.GetClick()).Take(1).Subscribe(notUsed => PostTweet());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string url = null, text = "";
            if (NavigationContext.QueryString.ContainsKey("url") && NavigationContext.QueryString.ContainsKey("text"))
            {
                url = NavigationContext.QueryString["url"];
                text = NavigationContext.QueryString["text"];
            }
            if (!string.IsNullOrEmpty(url))
            {
                shorteningTextBlurb.Visibility = Visibility.Visible;
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;

                UrlShortenerService.GetShortenedUrl(url)
                    .ObserveOnDispatcher()
                    .Subscribe(
                        shortenedUrl =>
                        {
                            ProgressBar.Visibility = Visibility.Collapsed;
                            ProgressBar.IsIndeterminate = false;

                            shorteningTextBlurb.Visibility = Visibility.Collapsed;
                            string message = string.Format("{0} {1}", text, shortenedUrl);
                            TweetTextBox.Text = message;
                            TweetTextBox.IsEnabled = true;
                        },
                        exception => HandleUrlShortenerException(exception));
            }

            // this would be an error state - both uri and url were either null or malformed
            else if (NavigationService.CanGoBack && !navigatedAway)
            {
                MessageBox.Show("Something is wrong with that article's link.");
                NavigationService.GoBack();
            }
        }

        void HandleUrlShortenerException(Exception exception)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
        }

        void UpdateRemainingCharacters()
        {
            CharactersCountTextBlock.Text = String.Format("{0} characters remaining", 140 - TweetTextBox.Text.Length);
        }

        void TweetTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRemainingCharacters();
        }

        void MessageTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Focus();
                PostTweet();
            }
        }

        void PostTweet()
        {
            if (String.IsNullOrEmpty(TweetTextBox.Text))
                return;

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            TwitterPostService.PostTweet(TweetTextBox.Text, TwitterAccount.CurrentTwitterAccessCredentials)
                .ObserveOnDispatcher()
                .Subscribe(
                    notUsed =>
                    {
                        ProgressBar.Visibility = Visibility.Collapsed;
                        ProgressBar.IsIndeterminate = false;

                        ToastService.ToastPrompt("Tweet sent!");

                        //MessageBox.Show("Tweet sent!");
                        if (NavigationService.CanGoBack && !navigatedAway)
                            NavigationService.GoBack();
                    },
                    exception => HandleTweetException(exception));
        }

        void HandleTweetException(Exception exception)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;

            if (exception is UnauthorizedCredentialsException)
            {
                MessageBox.Show(
                    string.Format(
                    "There is an error with your Twitter credentials - did you recently revoke access to {0}?  Please re-enter your Twitter username and password",
                    AppSettings.AppName));

                TwitterAccount.CurrentTwitterAccessCredentials = null;
                authPage.Visibility = Visibility.Visible;
                authPage.BeginOAuthWorkflow();
            }
            else
                MessageBox.Show("There was an error connecting to Twitter");
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            navigatedAway = true;
        }
    }
}