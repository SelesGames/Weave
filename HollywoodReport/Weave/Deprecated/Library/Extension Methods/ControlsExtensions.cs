using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Controls
{
    public static class ControlsExtensions
    {
        public static void Show(this ProgressBar progressBar)
        {
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;
        }

        public static void Hide(this ProgressBar progressBar)
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
        }

        public static IObservable<IEvent<RoutedEventArgs>> GetClick(this Button button)
        {
            return Observable.FromEvent<RoutedEventArgs>(button, "Click");
        }
    }
}
