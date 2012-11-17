using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.ComponentModel;

namespace System.Reactive.Concurrency
{
    public sealed class OneThreadScheduler : IScheduler
    {
        // Fields
        internal readonly object gate = new object();
        internal readonly Dictionary<Timer, object> timers = new Dictionary<Timer, object>();

        Pool pool;
        // Methods
        public OneThreadScheduler(int numberOfThreads)
        {
            pool = new Pool(numberOfThreads);
        }

        public IDisposable Schedule(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            BooleanDisposable cancelable = new BooleanDisposable();
            //ThreadPool.QueueUserWorkItem(delegate(object _)
            //{
            //    if (!cancelable.IsDisposed)
            //    {
            //        action();
            //    }
            //}, null);
            pool.QueueTask(() =>
            {
                if (!cancelable.IsDisposed)
                {
                    action();
                }
            });
            return cancelable;
        }

        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            TimeSpan span = Normalize(dueTime);
            bool hasAdded = false;
            bool hasRemoved = false;
            Timer timer = null;
            timer = new Timer(delegate(object notUsed)
                {
                    lock (gate)
                    {
                        if (hasAdded && (timer != null))
                        {
                            timers.Remove(timer);
                        }
                        hasRemoved = true;
                    }
                    timer = null;
                    action();
                },
                null,
                span,
                TimeSpan.FromMilliseconds(-1.0));

            lock (gate)
            {
                if (!hasRemoved)
                {
                    timers.Add(timer, null);
                    hasAdded = true;
                }
            }
            return new AnonymousDisposable(delegate
            {
                Timer key = timer;
                if (key != null)
                {
                    key.Dispose();
                    lock (gate)
                    {
                        timers.Remove(key);
                        hasRemoved = true;
                    }
                }
                timer = null;
            });
        }

        // Properties
        public DateTimeOffset Now
        {
            get
            {
                return DateTimeOffset.Now;
            }
        }

        internal static TimeSpan Normalize(TimeSpan timeSpan)
        {
            if (timeSpan.Ticks < 0L)
            {
                return TimeSpan.Zero;
            }
            return timeSpan;
        }


        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class AnonymousDisposable : IDisposable
    {
        // Fields
        private readonly Action dispose;
        private bool isDisposed;

        // Methods
        public AnonymousDisposable(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.dispose();
            }
        }
    }

    public sealed class Pool : IDisposable
    {
        private readonly LinkedList<Thread> _workers;
        private readonly LinkedList<Action> _tasks = new LinkedList<Action>();
        private bool _disallowAdd;
        private bool _disposed;

        public Pool(int size)
        {
            this._workers = new LinkedList<Thread>();
            for (var i = 0; i < size; ++i)
            {
                var worker = new Thread(this.Worker) { Name = string.Concat("Worker ", i), IsBackground = true };
                worker.Start();
                this._workers.AddLast(worker);
            }
        }

        public Pool(string threadName)
        {
            this._workers = new LinkedList<Thread>();
            var worker = new Thread(this.Worker) { Name = threadName, IsBackground = true };
            worker.Start();
            this._workers.AddLast(worker);
        }

        public void Dispose()
        {
            var waitForThreads = false;
            lock (this._tasks)
            {
                if (!this._disposed)
                {
                    GC.SuppressFinalize(this);

                    this._disallowAdd = true; // wait for all tasks to finish processing while not allowing any more new tasks
                    while (this._tasks.Count > 0)
                    {
                        Monitor.Wait(this._tasks);
                    }

                    this._disposed = true;
                    Monitor.PulseAll(this._tasks); // wake all workers (none of them will be active at this point; disposed flag will cause then to finish so that we can join them)
                    waitForThreads = true;
                }
            }
            if (waitForThreads)
            {
                foreach (var worker in this._workers)
                {
                    worker.Join();
                }
            }
        }

        public void QueueTask(Action task)
        {
            lock (this._tasks)
            {
                if (this._disallowAdd) { throw new InvalidOperationException("This Pool instance is in the process of being disposed, can't add anymore"); }
                if (this._disposed) { throw new ObjectDisposedException("This Pool instance has already been disposed"); }
                this._tasks.AddLast(task);
                Monitor.PulseAll(this._tasks); // pulse because tasks count changed
            }
        }

        private void Worker()
        {
            Action task = null;
            while (true)
            {
                lock (this._tasks)
                {
                    if (this._disposed)
                    {
                        return;
                    }
                    if (null != task)
                    {
                        this._workers.AddLast(Thread.CurrentThread);
                        task = null;
                    }
                    if (null != this._workers.First && object.ReferenceEquals(Thread.CurrentThread, this._workers.First.Value))
                    {
                        if (this._tasks.Count > 0)
                        {
                            task = this._tasks.First.Value;
                            this._tasks.RemoveFirst();
                            this._workers.RemoveFirst();
                            Monitor.PulseAll(this._tasks); // pulse because current (First) worker changed
                        }
                    }
                    if (task == null)
                    {
                        Monitor.Wait(this._tasks);
                        continue;
                    }
                }

                //try
                //{
                task();
                //}
                //catch (Exception ex) { }
            }
        }
    }

    public sealed class SingleThreadScheduler : IScheduler
    {
        // Fields
        internal static readonly object gate = new object();
        //internal static readonly OneThreadScheduler2 Instance = new OneThreadScheduler2();
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

    //public class BackgroundWorkerScheduler : IScheduler
    //{
    //    BackgroundWorker bgWorker = new BackgroundWorker();

    //    public BackgroundWorkerScheduler Instance = new BackgroundWorkerScheduler();

    //    private BackgroundWorkerScheduler()
    //    {

    //    }

    //    public DateTimeOffset Now
    //    {
    //        get { return Scheduler.Now; }
    //    }

    //    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
    //    {
    //        if (action == null)
    //        {
    //            throw new ArgumentNullException("action");
    //        }
    //        return this.Schedule<TState>(state, (TimeSpan)(dueTime - this.Now), action);
    //    }

    //    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    //    {
    //        bgWorker.
    //    }
    //}
}
