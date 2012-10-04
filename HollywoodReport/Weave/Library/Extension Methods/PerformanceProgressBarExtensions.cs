using System.Windows;

namespace Microsoft.Phone.Controls
{
    internal static class PerformanceProgressBarExtensions
    {
        internal static void Show(this PerformanceProgressBar progressBar)
        {
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;
        }

        internal static void Hide(this PerformanceProgressBar progressBar)
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
        }
    }
}
