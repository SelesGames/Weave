using Microsoft.Phone.Controls;
using System;
using System.Windows.Navigation;

namespace weave
{
    public class MainPageSourcesRefresher : IDisposable
    {
        PhoneApplicationFrame frame;
        MainPage activeMainPage;
        PhoneApplicationPage currentPage, lastPage;

        public MainPageSourcesRefresher(PhoneApplicationFrame frame)
        {
            this.frame = frame;
            frame.Navigated += OnFrameNavigated;
            frame.Navigating += OnFrameNavigating;
        }

        async void OnFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var uri = e.Uri;
            if (activeMainPage != null && 
                e.IsCancelable &&
                uri.OriginalString.StartsWith("/weave;component/Pages/MainPage/MainPage.xaml") &&
                e.NavigationMode == NavigationMode.New)
            {
                e.Cancel = true;
                await activeMainPage.OnNavigatedTo(uri, NavigationMode.Refresh);
            }
        }

        void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!(e.Content is PhoneApplicationPage))
                return;

            lastPage = currentPage;
            currentPage = (PhoneApplicationPage)e.Content;

            if (e.NavigationMode == NavigationMode.New && currentPage is MainPage)
            {
                DebugEx.WriteLine("activeMainPage SET");
                activeMainPage = (MainPage)currentPage;
            }
            else if (e.NavigationMode == NavigationMode.Back && lastPage == activeMainPage)
            {
                DebugEx.WriteLine("activeMainPage set to null");
                activeMainPage = null;
            }
        }

        public void Dispose()
        {
            frame.Navigated -= OnFrameNavigated;
            frame.Navigating -= OnFrameNavigating;
        }
    }
}
