using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

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

        public static IObservable<EventPattern<MouseButtonEventArgs>> GetMouseLeftButtonUp(this UIElement el)
        {
            return Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>
                (h => new MouseButtonEventHandler(h),
                    h => el.MouseLeftButtonUp += h,
                    h => el.MouseLeftButtonUp -= h
                 );
        }

        //public static IObservable<EventPattern<MouseButtonEventArgs>> GetSuperMouseLeftButtonDown(this UIElement el)
        //{
        //    return Observable.Create<EventPattern<MouseButtonEventArgs>>(observer =>
        //    {
        //        MouseButtonEventHandler handler = (s, e) => observer.OnNext(EventPattern.Create(s, e));

        //        try
        //        {
        //            el.AddHandler(UIElement.MouseLeftButtonDownEvent, handler, true);
        //        }
        //        catch (Exception exception)
        //        {
        //            observer.OnError(exception);
        //        }

        //        return () => el.RemoveHandler(UIElement.MouseLeftButtonDownEvent, handler);
        //    });
        //}

        //public static IObservable<EventPattern<MouseButtonEventArgs>> GetSuperMouseLeftButtonUp(this UIElement el)
        //{
        //    return Observable.Create<EventPattern<MouseButtonEventArgs>>(observer =>
        //    {
        //        MouseButtonEventHandler handler = (s, e) => observer.OnNext(EventPattern.Create(s, e));

        //        try
        //        {
        //            el.AddHandler(UIElement.MouseLeftButtonUpEvent, handler, true);
        //        }
        //        catch (Exception exception)
        //        {
        //            observer.OnError(exception);
        //        }

        //        return () => el.RemoveHandler(UIElement.MouseLeftButtonUpEvent, handler);
        //    });
        //}

        public static IObservable<EventPattern<MouseButtonEventArgs>> GetMouseLeftButtonDown(this UIElement el)
        {
            return Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>
                (h => new MouseButtonEventHandler(h),
                    h => el.MouseLeftButtonDown += h,
                    h => el.MouseLeftButtonDown -= h
                 );
        }

        public static IObservable<EventPattern<MouseEventArgs>> GetMouseMove(this UIElement el)
        {
            return Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>
                (h => new MouseEventHandler(h),
                    h => el.MouseMove += h,
                    h => el.MouseMove -= h
                 );
        }

        public static IObservable<EventPattern<ManipulationStartedEventArgs>> GetManipulationStarted(this UIElement el)
        {
            return Observable.FromEventPattern<EventHandler<ManipulationStartedEventArgs>, ManipulationStartedEventArgs>
                (h => new EventHandler<ManipulationStartedEventArgs>(h),
                    h => el.ManipulationStarted += h,
                    h => el.ManipulationStarted -= h);
        }

        public static IObservable<EventPattern<ManipulationDeltaEventArgs>> GetManipulationDelta(this UIElement el)
        {
            return Observable.FromEventPattern<EventHandler<ManipulationDeltaEventArgs>, ManipulationDeltaEventArgs>
                (h => new EventHandler<ManipulationDeltaEventArgs>(h),
                    h => el.ManipulationDelta += h,
                    h => el.ManipulationDelta -= h);
        }

        public static IObservable<EventPattern<ManipulationCompletedEventArgs>> GetManipulationCompleted(this UIElement el)
        {
            return Observable.FromEventPattern<EventHandler<ManipulationCompletedEventArgs>, ManipulationCompletedEventArgs>
                (h => new EventHandler<ManipulationCompletedEventArgs>(h),
                    h => el.ManipulationCompleted += h,
                    h => el.ManipulationCompleted -= h);
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
