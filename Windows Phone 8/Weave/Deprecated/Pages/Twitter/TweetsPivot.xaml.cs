using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using weave.Services.Twitter;

namespace weave
{
    public partial class TweetsPivot : PhoneApplicationPage
    {
        ObservableCollection<Tweet> tweetsSource = new ObservableCollection<Tweet>();
        int pageIndex = 1;

        TwitterClient client;

        public TweetsPivot()
        {
            InitializeComponent();
            longList.ItemsSource = tweetsSource;
            longList.ShowListFooter = false;

            client = new TwitterClient(TwitterSettings.ConsumerKey, TwitterSettings.ConsumerKeySecret, TwitterAccount.CurrentTwitterAccessCredentials);

            this.GetLoaded().Take(1).Subscribe(notUsed =>
                RequestTweets());
        }

        void RequestTweets()
        {
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;

            client.GetTimeLine().ObserveOnDispatcher().Subscribe(
                LoadTweets,
                HandleException);
        }

        void GetMoreTweets()
        {
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;

            longList.ShowListFooter = false;
            pageIndex++;
            //TweetService.GetRegularTweetsRequest(pageIndex).ObserveOnDispatcher().Subscribe(
            //    LoadTweets,
            //    HandleException);
        }

        void LoadTweets(IEnumerable<Tweet> tweets)
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;

            longList.ItemsSource = tweets.ToList();
            foreach (var tweet in tweets)
                tweetsSource.Add(tweet);

            longList.ShowListFooter = true;
        }

        void HandleException(Exception exception)
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
        }

        void longList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (longList.SelectedItem == null)
                return;

            var tweet = longList.SelectedItem as Tweet;
            longList.SelectedItem = null;

            //LocalNavigationService.ToIntermediateTweetPage(tweet.Html);
        }

        void getMoreTweetsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GetMoreTweets();
        }
    }
}