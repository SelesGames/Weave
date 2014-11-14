using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace System.Windows
{
    public static partial class UIElementExtensions
    {
        public static IObservable<EventPattern<MouseButtonEventArgs>> GetMouseLeftButtonUp(this UIElement el)
        {
            return Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>
                (h => new MouseButtonEventHandler(h),
                    h => el.MouseLeftButtonUp += h,
                    h => el.MouseLeftButtonUp -= h
                 );
        }

        public static IObservable<EventPattern<MouseButtonEventArgs>> GetSuperMouseLeftButtonDown(this UIElement el)
        {
            return Observable.Create<EventPattern<MouseButtonEventArgs>>(observer =>
            {
                MouseButtonEventHandler handler = (s, e) => observer.OnNext(EventPattern.Create(s, e));

                try
                {
                    el.AddHandler(UIElement.MouseLeftButtonDownEvent, handler, true);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return () => el.RemoveHandler(UIElement.MouseLeftButtonDownEvent, handler);
            });
        }

        public static IObservable<EventPattern<MouseButtonEventArgs>> GetSuperMouseLeftButtonUp(this UIElement el)
        {
            return Observable.Create<EventPattern<MouseButtonEventArgs>>(observer =>
            {
                MouseButtonEventHandler handler = (s, e) => observer.OnNext(EventPattern.Create(s, e));

                try
                {
                    el.AddHandler(UIElement.MouseLeftButtonUpEvent, handler, true);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }

                return () => el.RemoveHandler(UIElement.MouseLeftButtonUpEvent, handler);
            });
        }

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
    }
}
