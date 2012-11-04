using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media;

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

        //public static IObservable<EventPattern<MouseEventArgs>> GetMouseLeave(this UIElement el)
        //{
        //    return Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>
        //        (h => new MouseEventHandler(h),
        //            h => el.MouseLeave += h,
        //            h => el.MouseLeave -= h
        //         );
        //}

        //public static IObservable<Unit> GetMouseLeftButtonTap(this UIElement el)
        //{
        //    //return el.GetMouseLeftButtonTap(TimeSpan.FromMilliseconds(300), 30d, 15d);
        //    return el.GetMouseLeftButtonTap(TimeSpan.FromMilliseconds(250), 20d, 10d);
        //}


        //public static IObservable<Unit> GetMouseLeftButtonTap(
        //    this UIElement el, TimeSpan maxTapDuration, double maxXMovement, double maxYMovement)
        //{
        //    var mouseDown = el.GetMouseLeftButtonDown()
        //        .Select(o => new { time = DateTime.Now, pos = o.EventArgs.GetSafePosition(el) });
        //    var mouseUp = el.GetMouseLeftButtonUp()
        //        .Select(o => new { time = DateTime.Now, pos = o.EventArgs.GetSafePosition(el) });

        //    var tap = from md in mouseDown
        //              from mu in mouseUp
        //              select new
        //              {
        //                  elapsedTime = mu.time - md.time,
        //                  delta = new Point(mu.pos.X - md.pos.X, mu.pos.Y - md.pos.Y)
        //              };

        //    return tap.Where(o =>
        //            o.elapsedTime < maxTapDuration &&
        //            Math.Abs(o.delta.X) < maxXMovement &&
        //            Math.Abs(o.delta.Y) < maxYMovement)
        //        .Select(o => Unit.Default);
        //}

        //public static IObservable<Unit> GetSuperMouseLeftButtonTap(this UIElement el)
        //{
        //    //return el.GetMouseLeftButtonTap(TimeSpan.FromMilliseconds(300), 30d, 15d);
        //    return el.GetSuperMouseLeftButtonTap(TimeSpan.FromMilliseconds(250), 20d, 10d);
        //}


        //public static IObservable<Unit> GetSuperMouseLeftButtonTap(
        //    this UIElement el, TimeSpan maxTapDuration, double maxXMovement, double maxYMovement)
        //{
        //    var mouseDown = el.GetSuperMouseLeftButtonDown()
        //        .Select(o => new { time = DateTime.Now, pos = o.EventArgs.GetSafePosition(el) });
        //    var mouseUp = el.GetSuperMouseLeftButtonUp()
        //        .Select(o => new { time = DateTime.Now, pos = o.EventArgs.GetSafePosition(el) });

        //    var tap = from md in mouseDown
        //              from mu in mouseUp
        //              select new
        //              {
        //                  elapsedTime = mu.time - md.time,
        //                  delta = new Point(mu.pos.X - md.pos.X, mu.pos.Y - md.pos.Y)
        //              };

        //    return tap.Where(o =>
        //            o.elapsedTime < maxTapDuration &&
        //            Math.Abs(o.delta.X) < maxXMovement &&
        //            Math.Abs(o.delta.Y) < maxYMovement)
        //        .Select(o => Unit.Default);
        //}

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

        //public static IObservable<Unit> GetManipulationTap(this UIElement el, int maxElapsedMilliseconds = 250)
        //{
        //    return Observable.Create<Unit>(observer =>
        //    {
        //        var totalDelta = new Point();
        //        var disposables = new CompositeDisposable();

        //        var disp = el.GetManipulationDelta()
        //            .Do(e =>
        //            {
        //                totalDelta = new Point(
        //                    totalDelta.X + e.EventArgs.DeltaManipulation.Translation.X,
        //                    totalDelta.Y + e.EventArgs.DeltaManipulation.Translation.Y);
        //            })
        //            .Subscribe(o => { ; }, ex => observer.OnError(ex));

        //        disposables.Add(disp);

        //        var subscription = from x in el.GetManipulationStarted()
        //                               .Do(o => totalDelta = new Point())
        //                               .Select(o => new { mouseDown = DateTime.Now })

        //                           from y in el.GetManipulationCompleted().Select(o => new { mouseUp = DateTime.Now })

        //                           select new
        //                           {
        //                               elapsedTime = y.mouseUp - x.mouseDown,
        //                           };

        //        disp = subscription
        //            .Take(1)
        //            .Repeat()
        //            .Where(o =>
        //                Math.Abs(totalDelta.X) < 4d &&
        //                Math.Abs(totalDelta.Y) < 4d &&
        //                o.elapsedTime < TimeSpan.FromMilliseconds(maxElapsedMilliseconds))
        //            .Subscribe(notUsed => observer.OnNext(new Unit()), ex => observer.OnError(ex));

        //        disposables.Add(disp);

        //        return disposables;
        //    });
        //}

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

        //public static IObservable<EventPattern<EventArgs>> GetFirstLayoutUpdated(this FrameworkElement frameworkElement)
        //{
        //    var subject = new AsyncSubject<EventPattern<EventArgs>>();
        //    var disp = new CompositeDisposable();

        //    try
        //    {
        //        bool hasRunOnce = false;
        //        var sync = new object();

        //        EventHandler handler = null;
        //        handler = new EventHandler((s, e) =>
        //         {
        //             try
        //             {
        //                 lock (sync)
        //                 {
        //                     if (hasRunOnce)
        //                         return;
        //                     else
        //                         hasRunOnce = true;
        //                 }
        //                 frameworkElement.LayoutUpdated -= handler;
        //                 subject.OnNext(EventPattern.Create(frameworkElement, e));
        //                 subject.OnCompleted();
        //             }
        //             catch (Exception exception)
        //             {
        //                 subject.OnError(exception);
        //             }
        //         });

        //        frameworkElement.LayoutUpdated += handler;
        //    }
        //    catch (Exception exception)
        //    {
        //        subject.OnError(exception);
        //    }
        //    return Observable.Create<EventPattern<EventArgs>>(observer =>
        //    {
        //        try
        //        {
        //            subject.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted)
        //                .DisposeWith(disp);
        //        }
        //        catch (Exception exception)
        //        {
        //            observer.OnError(exception);
        //        }
        //        return disp;
        //    });
        //}

        //public static IObservable<T> GetTapOnItemOfType<T>(this UIElement el, UIElement relativeTo) where T : UIElement
        //{
        //    return el.GetTap()
        //        .Select(o =>
        //        {
        //            var sw = System.Diagnostics.Stopwatch.StartNew();
        //            var pos = o.EventArgs.GetPosition(relativeTo);
        //            var ui = VisualTreeHelper.FindElementsInHostCoordinates(pos, relativeTo).OfType<T>().FirstOrDefault();

        //            sw.Stop();
        //            DebugEx.WriteLine("took {0} ms to process tap", sw.ElapsedMilliseconds);
        //            return ui;
        //        })
        //        .Where(o => o != null);
        //}
    }
}
