using System.Collections.Generic;
using System.Disposables;
using System.Linq;
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

        public static IObservable<IEvent<MouseButtonEventArgs>> GetMouseLeftButtonUp(this UIElement el)
        {
            return Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>
                (h => new MouseButtonEventHandler(h),
                    h => el.MouseLeftButtonUp += h,
                    h => el.MouseLeftButtonUp -= h
                 );
        }

        public static IObservable<IEvent<MouseButtonEventArgs>> GetSuperMouseLeftButtonUp(this UIElement el)
        {
            return Observable.Create<IEvent<MouseButtonEventArgs>>(observer =>
            {
                MouseButtonEventHandler handler = (s,e) => observer.OnNext(Event.Create(s, e));

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

        public static IObservable<IEvent<MouseButtonEventArgs>> GetMouseLeftButtonDown(this UIElement el)
        {
            return Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>
                (h => new MouseButtonEventHandler(h),
                    h => el.MouseLeftButtonDown += h,
                    h => el.MouseLeftButtonDown -= h
                 );
        }

        public static IObservable<IEvent<MouseEventArgs>> GetMouseMove(this UIElement el)
        {
            return Observable.FromEvent<MouseEventHandler, MouseEventArgs>
                (h => new MouseEventHandler(h),
                    h => el.MouseMove += h,
                    h => el.MouseMove -= h
                 );
        }

        public static IObservable<IEvent<MouseEventArgs>> GetMouseLeave(this UIElement el)
        {
            return Observable.FromEvent<MouseEventHandler, MouseEventArgs>
                (h => new MouseEventHandler(h),
                    h => el.MouseLeave += h,
                    h => el.MouseLeave -= h
                 );
        }

        public static IObservable<Unit> GetMouseLeftButtonTap(this UIElement el)
        {
            //return el.GetMouseLeftButtonTap(TimeSpan.FromMilliseconds(300), 30d, 15d);
            return el.GetMouseLeftButtonTap(TimeSpan.FromMilliseconds(250), 20d, 10d);
        }


        public static IObservable<Unit> GetMouseLeftButtonTap(
            this UIElement el, TimeSpan maxTapDuration, double maxXMovement, double maxYMovement)
        {
            var mouseDown = el.GetMouseLeftButtonDown()
                .Select(o => new { time = DateTime.Now, pos = o.EventArgs.GetSafePosition(el) });
            var mouseUp = el.GetMouseLeftButtonUp()
                .Select(o => new { time = DateTime.Now, pos = o.EventArgs.GetSafePosition(el) });

            var tap = from md in mouseDown
                      from mu in mouseUp
                      select new
                      {
                          elapsedTime = mu.time - md.time,
                          delta = new Point(mu.pos.X - md.pos.X, mu.pos.Y - md.pos.Y)
                      };

            return tap.Where(o =>
                    o.elapsedTime < maxTapDuration &&
                    Math.Abs(o.delta.X) < maxXMovement &&
                    Math.Abs(o.delta.Y) < maxYMovement)
                .Select(o => new Unit());
        }

        public static IObservable<IEvent<ManipulationStartedEventArgs>> GetManipulationStarted(this UIElement el)
        {
            return Observable.FromEvent<EventHandler<ManipulationStartedEventArgs>, ManipulationStartedEventArgs>
                (h => new EventHandler<ManipulationStartedEventArgs>(h),
                    h => el.ManipulationStarted += h,
                    h => el.ManipulationStarted -= h);
        }

        public static IObservable<IEvent<ManipulationDeltaEventArgs>> GetManipulationDelta(this UIElement el)
        {
            return Observable.FromEvent<EventHandler<ManipulationDeltaEventArgs>, ManipulationDeltaEventArgs>
                (h => new EventHandler<ManipulationDeltaEventArgs>(h),
                    h => el.ManipulationDelta += h,
                    h => el.ManipulationDelta -= h);
        }

        public static IObservable<IEvent<ManipulationCompletedEventArgs>> GetManipulationCompleted(this UIElement el)
        {
            return Observable.FromEvent<EventHandler<ManipulationCompletedEventArgs>, ManipulationCompletedEventArgs>
                (h => new EventHandler<ManipulationCompletedEventArgs>(h),
                    h => el.ManipulationCompleted += h,
                    h => el.ManipulationCompleted -= h);
        }

        public static IObservable<Unit> GetManipulationTap(this UIElement el, int maxElapsedMilliseconds = 250)
        {
            return Observable.CreateWithDisposable<Unit>(observer =>
            {
                var totalDelta = new Point();
                var disposables = new CompositeDisposable();

                var disp = el.GetManipulationDelta()
                    .Do(e =>
                    {
                        totalDelta = new Point(
                            totalDelta.X + e.EventArgs.DeltaManipulation.Translation.X,
                            totalDelta.Y + e.EventArgs.DeltaManipulation.Translation.Y);
                    })
                    .Subscribe(o => { ; }, ex => observer.OnError(ex));

                disposables.Add(disp);

                var subscription = from x in el.GetManipulationStarted()
                                       .Do(o => totalDelta = new Point())
                                       .Select(o => new { mouseDown = DateTime.Now })

                                   from y in el.GetManipulationCompleted().Select(o => new { mouseUp = DateTime.Now })

                                   select new
                                   {
                                       elapsedTime = y.mouseUp - x.mouseDown,
                                   };

                disp = subscription
                    .Take(1)
                    .Repeat()
                    .Where(o =>
                        Math.Abs(totalDelta.X) < 4d &&
                        Math.Abs(totalDelta.Y) < 4d &&
                        o.elapsedTime < TimeSpan.FromMilliseconds(maxElapsedMilliseconds))
                    .Subscribe(notUsed => observer.OnNext(new Unit()), ex => observer.OnError(ex));

                disposables.Add(disp);

                return disposables;
            });
        }

        public static IObservable<IEvent<RoutedEventArgs>> GetLoaded(this FrameworkElement frameworkElement)
        {
            return Observable.FromEvent<RoutedEventArgs>(frameworkElement, "Loaded");
        }

        public static IObservable<IEvent<EventArgs>> GetLayoutUpdated(this FrameworkElement frameworkElement)
        {
            return Observable.FromEvent<EventArgs>(frameworkElement, "LayoutUpdated");
        }
    }
}
