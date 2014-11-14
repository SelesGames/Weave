using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace System.Windows
{
    public static partial class UIElementExtensions
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

        public static IObservable<EventPattern<GestureEventArgs>> GetTap(this UIElement el)
        {
            return Observable.FromEventPattern<GestureEventArgs>(el, "Tap");
        }

        public static IObservable<EventPattern<GestureEventArgs>> GetHold(this UIElement el)
        {
            return Observable.FromEventPattern<GestureEventArgs>(el, "Hold");
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
