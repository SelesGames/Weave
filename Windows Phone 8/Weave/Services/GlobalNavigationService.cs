using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using SelesGames.Phone;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Weave.ViewModels;

namespace weave
{
    public static class GlobalNavigationService
    {
        public static PhoneApplicationFrame CurrentFrame { get; set; }

        static void SafelyNavigateTo(string uri)
        {         
            CurrentFrame.TryNavigate(uri);
        }

        public static void ToPanoramaPage()
        {
            SafelyNavigateTo("/weave;component/Pages/Panorama/SamplePanorama.xaml");
        }

        public static void ToWelcomePage()
        {
            SafelyNavigateTo("/weave;component/Pages/SelectTheCategoriesThatInterestYouPage.xaml");
        }

        public static void ToMainPage(string header, string mode)
        {
            var urlEncodedHeader = System.Net.HttpUtility.UrlEncode(header);
            SafelyNavigateTo(string.Format("/weave;component/Pages/MainPage/MainPage.xaml?header={0}&mode={1}", urlEncodedHeader, mode));
        }

        public static void ToMainPage(string header, Guid feedId)
        {
            var urlEncodedHeader = System.Net.HttpUtility.UrlEncode(header);
            SafelyNavigateTo(string.Format("/weave;component/Pages/MainPage/MainPage.xaml?header={0}&feedId={1}", urlEncodedHeader, feedId));
        }

        public static void ToInstapaperAccountCredentialsPage()
        {
            SafelyNavigateTo("/weave;component/Pages/Accounts/InstapaperAccountCredentialsPage.xaml");
        }

        public static void ToWebBrowserPage(NewsItem newsItem)
        {
            if (newsItem == null || newsItem.Feed == null)
                return;

            var vm = new ReadabilityPageViewModel { NewsItem = newsItem };

            //var articleViewType = newsItem.FeedSource.ArticleViewingType;
            //if (articleViewType == ArticleViewingType.Mobilizer || articleViewType == ArticleViewingType.MobilizerOnly)
            //    vm.PreLoadMobilizedHtml();

            var ts = AppSettings.Instance.TombstoneState.Get().WaitOnResult();
            ts.ActiveWebBrowserPageViewModel = vm;
            SafelyNavigateTo(string.Format("/weave;component/Pages/WebBrowser/ReadabilityPage.xaml"));
        }

        public static void ToInternetExplorer(INewsItem newsItem)
        {
            //new WebBrowserTask { Uri = Uri.EscapeDataString(newsItem.Link).ToUri() }.Show();
            if (newsItem != null && newsItem.Link != null)
                ToInternetExplorer(newsItem.Link);

            else
                MessageBox.Show("We apologize, there seems to be something wrong with that article's link!");
        }

        public static void ToInternetExplorer(string link)
        {
            try
            {
                var uri = link.ToUri();

                if (uri != null)
                    new WebBrowserTask { Uri = uri }.Show();

                else
                    MessageBox.Show("We apologize, there seems to be something wrong with that link!");
            }
            catch (Exception)
            {
                MessageBox.Show("We apologize, something went wrong when trying to open Internet Explorer!");
            }
        }

        public static void ToInfoAndSupportPage()
        {
            SafelyNavigateTo("/weave;component/Pages/Settings/InfoAndSupport.xaml");
        }

        public static void ToMainPageSettingsPage()
        {
            SafelyNavigateTo("/weave;component/Pages/MainPage/Settings/MainPageSettingsPage.xaml");
        }

        public static void ToChangeLogAndComingSoonPage()
        {
            SafelyNavigateTo("/weave;component/Pages/Settings/ChangelogAndComingSoonPage.xaml");
        }

        public static void ToDummyPage()
        {
            SafelyNavigateTo("/weave;component/Pages/DummyPage.xaml");
        }

        public static void ToAppSettingsPage()
        {
            SafelyNavigateTo("/weave;component/Pages/AppSettingsPage.xaml");
        }

        public static void ToSelesGamesInfoPage()
        {
            SafelyNavigateTo("/weave;component/Pages/SelesGamesInfoPage.xaml");
        }
    }
}
