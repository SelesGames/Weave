using System.Reactive;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace System.Windows
{
    public static class UIElementExtensions
    {
        public static void SoftCollapse(this UIElement element)
        {
            element.Opacity = 0;
            element.IsHitTestVisible = false;
        }

        public static void SoftMakeVisible(this UIElement element)
        {
            element.Opacity = 1;
            element.IsHitTestVisible = true;
        }

        public static IObservable<EventPattern<TappedRoutedEventArgs>> GetTap(this UIElement el)
        {
            return Observable.FromEventPattern<TappedRoutedEventArgs>(el, "Tapped");
        }

        public static IObservable<EventPattern<RoutedEventArgs>> GetLoaded(this FrameworkElement frameworkElement)
        {
            return Observable.FromEventPattern<RoutedEventArgs>(frameworkElement, "Loaded");
        }

        public static IObservable<EventPattern<RoutedEventArgs>> GetUnloaded(this FrameworkElement frameworkElement)
        {
            return Observable.FromEventPattern<RoutedEventArgs>(frameworkElement, "Unloaded");
        }

        public static IObservable<EventPattern<EventArgs>> GetLayoutUpdated(this FrameworkElement frameworkElement)
        {
            return Observable.FromEventPattern<EventArgs>(frameworkElement, "LayoutUpdated");
        }
    }
}
