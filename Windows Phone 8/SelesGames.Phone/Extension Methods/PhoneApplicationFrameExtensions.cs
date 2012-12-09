using Microsoft.Phone.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SelesGames.Phone
{
    public static class PhoneApplicationFrameExtensions
    {
        public static Task<NavigationEventArgs> NavigationStoppedAsync(this PhoneApplicationFrame frame)
        {
            var tcs = new TaskCompletionSource<NavigationEventArgs>();

            try
            {
                NavigationStoppedEventHandler handler = null;
                handler = (s, e) => 
                {
                    frame.NavigationStopped -= handler;
                    tcs.TrySetResult(e);
                };

                frame.NavigationStopped += handler;
            }

            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public static Task<NavigationEventArgs> NavigatedAsync(this PhoneApplicationFrame frame)
        {
            var tcs = new TaskCompletionSource<NavigationEventArgs>();

            try
            {
                NavigatedEventHandler handler = null;
                handler = (s, e) =>
                {
                    frame.Navigated -= handler;
                    tcs.TrySetResult(e);
                };

                frame.Navigated += handler;
            }

            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }
    }
}
