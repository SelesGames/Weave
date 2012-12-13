using Microsoft.Phone.Controls;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;

namespace weave
{
    public class ArticleListNavigationCorrector : IDisposable
    {
        PhoneApplicationFrame frame;
        PhoneApplicationPage currentPage;

        public ArticleListNavigationCorrector(PhoneApplicationFrame frame)
        {
            this.frame = frame;
            frame.Navigated += OnFrameNavigated;
        }

        async void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var lastPage = currentPage;
            currentPage = e.Content as PhoneApplicationPage;

            if (lastPage is MainPage && currentPage is MainPage)
                TryToRemoveLastPageFromBackStack((MainPage)lastPage);

            try
            {
                if (currentPage != null)
                    await currentPage.GetLoaded().Take(1).ToTask();
            }
            catch { }

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                using (var disp = lastPage as IDisposable) { }

            await Task.Run(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }

        void TryToRemoveLastPageFromBackStack(MainPage lastPage)
        {
            try
            {
                if (frame.BackStack.Any())
                    frame.RemoveBackEntry();
                lastPage.Dispose();
                lastPage = null;
            }
            catch { }
        }

        public void Dispose()
        {
            frame.Navigated -= OnFrameNavigated;
        }
    }
}
