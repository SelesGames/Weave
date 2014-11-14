using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Concurrency
{
    public sealed class SingleThreadScheduler : IScheduler
    {
        // Fields
        internal static readonly object gate = new object();
        internal static readonly Dictionary<Timer, object> timers = new Dictionary<Timer, object>();

        Pool pool;
        // Methods
        public SingleThreadScheduler()
        {
            pool = new Pool(1);
        }

        public SingleThreadScheduler(string threadName)
        {
            pool = new Pool(threadName);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            SingleAssignmentDisposable d = new SingleAssignmentDisposable();

            //ThreadPool.QueueUserWorkItem(delegate(object _)
            //{
            //    if (!d.IsDisposed)
            //    {
            //        d.Disposable = action.Invoke(this, state);
            //    }
            //}, null);

            pool.QueueTask(() =>
            {
                if (!d.IsDisposed)
                {
                    d.Disposable = action.Invoke(this, state);
                }
            });

            return d;
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            return this.Schedule<TState>(state, (TimeSpan)(dueTime - this.Now), action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            TimeSpan span = Scheduler.Normalize(dueTime);
            if (span.Ticks == 0L)
            {
                return this.Schedule<TState>(state, action);
            }
            bool hasAdded = false;
            bool hasRemoved = false;
            MultipleAssignmentDisposable d = new MultipleAssignmentDisposable();
            Timer timer = null;
            timer = new Timer(delegate(object _)
            {
                Func<IScheduler, TState, IDisposable> func1 = null;
                lock (gate)
                {
                    if (hasAdded && (timer != null))
                    {
                        timers.Remove(timer);
                    }
                    hasRemoved = true;
                    func1 = action;
                }
                Timer timer1 = timer;
                if (timer1 != null)
                {
                    timer1.Dispose();
                }
                timer = null;
                if (func1 != null)
                {
                    d.Disposable = func1.Invoke(this, state);
                }
                action = null;
            }, null, span, TimeSpan.FromMilliseconds(-1.0));
            lock (gate)
            {
                if (!hasRemoved)
                {
                    timers.Add(timer, null);
                    hasAdded = true;
                }
            }
            d.Disposable = Disposable.Create(delegate
            {
                try
                {
                    Timer key = timer;
                    if (key != null)
                    {
                        key.Dispose();
                        lock (gate)
                        {
                            timers.Remove(key);
                            hasRemoved = true;
                            action = null;
                        }
                    }
                    timer = null;
                }
                catch (Exception) { }
            });
            return d;
        }

        // Properties
        public DateTimeOffset Now
        {
            get
            {
                return Scheduler.Now;
            }
        }
    }
}