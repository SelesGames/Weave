using SelesGames;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Navigation;
using Weave.ViewModels;
using Weave.ViewModels.Helpers;

namespace weave
{
    public static class SettingsPagesNavigationService
    {
        static ViewModelLocator viewModelLocator = ServiceResolver.Get<ViewModelLocator>();

        public static void SafelyNavigateTo(NavigationService navService, string uri, params object[] args)
        {
            try
            {
                navService.Navigate(new Uri(string.Format(uri, args), UriKind.Relative));
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        public static void ToManageSourcesPage(this NavigationService navService)
        {
            SafelyNavigateTo(navService, "/Weave.UI.SettingsPages;component/Views/ManageSourcesPage.xaml");
        }

        public static void ToEditSourcePage(this NavigationService navService, Feed feed)
        {
            var key = Guid.NewGuid().ToString();
            viewModelLocator.Push(key, feed);

            SafelyNavigateTo(navService, "/Weave.UI.SettingsPages;component/Views/EditSourcePage.xaml?feedId={0}", key);
        }

        public static void ToAddSourcePage(this NavigationService navService, string section)
        {
            SafelyNavigateTo(navService, "/Weave.UI.SettingsPages;component/Views/AddSourcePage.xaml?section={0}", section);
        }

        public static void ToBrowseFeedsByCategoryPage(this NavigationService navService, string category)
        {
            var safeCategory = HttpUtility.UrlEncode(category);
            SafelyNavigateTo(navService, "/Weave.UI.SettingsPages;component/Views/BrowseFeedsByCategoryPage.xaml?category={0}", safeCategory);
        }
    }
}
