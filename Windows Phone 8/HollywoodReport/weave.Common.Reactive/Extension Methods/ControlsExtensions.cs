using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls
{
    public static class ControlsExtensions
    {
        //public static void Show(this ProgressBar progressBar)
        //{
        //    progressBar.IsIndeterminate = true;
        //    progressBar.Visibility = Visibility.Visible;
        //}

        //public static void Hide(this ProgressBar progressBar)
        //{
        //    progressBar.Visibility = Visibility.Collapsed;
        //    progressBar.IsIndeterminate = false;
        //}

        public static IObservable<EventPattern<RoutedEventArgs>> GetClick(this ButtonBase button)
        {
            return Observable.FromEventPattern<RoutedEventArgs>(button, "Click");
        }
    }
}
