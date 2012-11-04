using System;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Net;

namespace weave
{
    public static class GlobalNavigationService
    {
        public static void SafelyNavigateTo(NavigationService navService, string uri)
        {
            try
            {
                navService.Navigate(new Uri(uri, UriKind.Relative));
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        public static void ToManageSourcesPage(this NavigationService navService)
        {
            SafelyNavigateTo(navService, 
                "/Weave.UI.SettingsPages;component/Views/ManageSourcesPage.xaml");
        }

        public static void ToEditSourcePage(this NavigationService navService, string feedId)
        {
            SafelyNavigateTo(navService, string.Format(
                "/Weave.UI.SettingsPages;component/Views/EditSourcePage.xaml?feedId={0}", feedId));
        }

        public static void ToAddSourcePage(this NavigationService navService)
        {
            SafelyNavigateTo(navService,
                "/Weave.UI.SettingsPages;component/Views/AddSourcePage.xaml");
        }

        public static void ToBrowseFeedsByCategoryPage(this NavigationService navService, string category)
        {
            var safeCategory = HttpUtility.UrlEncode(category);
            SafelyNavigateTo(navService, string.Format(
                "/Weave.UI.SettingsPages;component/Views/BrowseFeedsByCategoryPage.xaml?category={0}", safeCategory));
        }
    }
}
