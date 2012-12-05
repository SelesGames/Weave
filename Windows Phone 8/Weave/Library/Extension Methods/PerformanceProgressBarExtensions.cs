
namespace System.Windows.Controls
{
    internal static class PerformanceProgressBarExtensions
    {
        internal static void Show(this ProgressBar progressBar)
        {
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;
        }

        internal static void Hide(this ProgressBar progressBar)
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
        }
    }
}
