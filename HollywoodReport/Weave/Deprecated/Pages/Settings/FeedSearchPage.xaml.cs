using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Weave.FeedSearchService;
using System.Linq;

namespace weave
{
    public partial class FeedSearchPage : PhoneApplicationPage
    {
        public FeedSearchPage()
        {
            InitializeComponent();
            progressBar.Collapse();
        }

        void SearchButton_Click(object sender, EventArgs e)
        {
            DoSearch();
        }

        void searchText_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DoSearch();
        }

        IDisposable existingSearchHandle = null;

        private void DoSearch()
        {
            string searchString = searchText.Text;
            if (string.IsNullOrEmpty(searchString))
                return;

            if (existingSearchHandle != null)
                existingSearchHandle.Dispose();

            searchPrompt.Visibility = Visibility.Collapsed;
            progressBar.MakeVisible();
            listBox.ItemsSource = null;
            listBox.Focus();

            var searchService = new FeedSearchService();
            existingSearchHandle = searchService.SearchForFeedsMatching(searchString)
                .ObserveOnDispatcher()
                .SafelySubscribe(o =>
                {
                    if (o.responseStatus == "200")
                    {
                        progressBar.Collapse();
                        listBox.ItemsSource = o.responseData.entries.Select(entry =>
                        {
                            entry.Sanitize();
                            return entry;
                        });
                    }
                    else if (o.responseStatus == "999")
                    {
                        progressBar.Collapse();
                        MessageBox.Show("There was an error contacting the search service.  Please make sure you have an internet connection and try again.");
                    }
                    else
                    {
                        progressBar.Collapse();
                        MessageBox.Show("Google's feed search service is down temporarily.  Please wait a few minutes, then try again.");
                    }
                },
                ex => Dispatcher.BeginInvoke(() => MessageBox.Show("There was an error while searching for the feed - please try again.")));
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var feed = (sender as FrameworkElement).DataContext as Entry;
            var newFeed = new FeedSource
            {
                FeedName = feed.title,
                FeedUri = feed.url,
            };
            //FeedSearchService.CurrentlyBeingEditedFeed = newFeed;
            //EditFeedPage.CurrentMode = EditFeedPage.Mode.New;
            //GlobalNavigationService.ToEditFeedPage();
        }
    }

    public static class ProgressBarExtensions
    {
        public static void Collapse(this ProgressBar progressBar)
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
        }

        public static void MakeVisible(this ProgressBar progressBar)
        {
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;
        }
    }
}