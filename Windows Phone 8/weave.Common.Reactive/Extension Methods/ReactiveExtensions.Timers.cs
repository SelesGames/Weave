using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace System.Reactive.Linq
{
    public static class ReactiveExtensions_Timers
    {
        public static IObservable<T> IntroducePeriod<T>(this IEnumerable<T> source, TimeSpan period)
        {
            return Observable.Interval(period, BackgroundTimer.Scheduler).Zip(source, (i, o) => o);
        }

        public static IObservable<T> IntroducePeriod<T>(this IEnumerable<T> source, TimeSpan dueTime, TimeSpan period)
        {
            return Observable.Timer(dueTime, period, BackgroundTimer.Scheduler).Zip(source, (i, o) => o);
        }

        public static IObservable<T> IntroducePeriod<T>(this IEnumerable<T> source, TimeSpan dueTime, TimeSpan period, IScheduler scheduler)
        {
            return Observable.Timer(dueTime, period, scheduler).Zip(source, (i, o) => o);
        }
    }
}
