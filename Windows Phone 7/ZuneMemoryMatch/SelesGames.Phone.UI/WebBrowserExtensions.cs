using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    public static class WebBrowserExtensions
    {
        public static IObservable<NavigationEventArgs> GetNavigated(this WebBrowser browser)
        {
            return Observable.FromEventPattern<NavigationEventArgs>(browser, "Navigated").Select(o => o.EventArgs);
        }

        public static IObservable<NavigatingEventArgs> GetNavigating(this WebBrowser browser)
        {
            return Observable.FromEventPattern<NavigatingEventArgs>(browser, "Navigating").Select(o => o.EventArgs);
        }

        public static IObservable<NavigationFailedEventArgs> GetNavigationFailed(this WebBrowser browser)
        {
            return Observable.FromEventPattern<NavigationFailedEventHandler, NavigationFailedEventArgs>(e => browser.NavigationFailed += e, e => browser.NavigationFailed -= e).Select(o => o.EventArgs);
        }

        public static IObservable<NavigationEventArgs> GetLoadCompleted(this WebBrowser browser)
        {
            return Observable.FromEventPattern<LoadCompletedEventHandler, NavigationEventArgs>(e => browser.LoadCompleted += e, e => browser.LoadCompleted -= e).Select(o => o.EventArgs);
        }

        public static Task<NavigationEventArgs> NavigateToStringAsync(this WebBrowser browser, string html)
        {
            var navigated = browser.GetNavigated().Take(1).ToTask();
            browser.NavigateToString(html);
            return navigated;
        }

        public static Task<NavigationEventArgs> NavigateAsync(this WebBrowser browser, Uri uri)
        {
            var navigated = browser.GetNavigated().Take(1).ToTask();
            browser.Navigate(uri);
            return navigated;
        }

        public static Task<NavigationEventArgs> NavigateAsync(this WebBrowser browser, Uri uri, byte[] postData, string headers)
        {
            var navigated = browser.GetNavigated().Take(1).ToTask();
            browser.Navigate(uri, postData, headers);
            return navigated;
        }
    }
}
