
namespace System.Reactive.Concurrency
{
    public sealed class SingleThreadScheduler : IScheduler
    {
        EventLoopScheduler innerScheduler;

        public SingleThreadScheduler()
        {
            innerScheduler = new EventLoopScheduler();
        }

        public SingleThreadScheduler(string threadName)
        {
            innerScheduler = new EventLoopScheduler();
        }

        public DateTimeOffset Now
        {
            get
            {
                return innerScheduler.Now;
            }
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return innerScheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return innerScheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return innerScheduler.Schedule(state, action);
        }
    }
}