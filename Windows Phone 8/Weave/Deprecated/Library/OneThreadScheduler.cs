using System.Collections.Generic;
using System.Disposables;
using System.Threading;

namespace System.Concurrency
{
    public sealed class OneThreadScheduler : IScheduler
    {
        // Fields
        internal readonly object gate = new object();
        internal static readonly OneThreadScheduler Instance = new OneThreadScheduler(1);
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
}
