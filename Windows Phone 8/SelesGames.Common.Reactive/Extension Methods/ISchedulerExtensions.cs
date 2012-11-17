
namespace System.Reactive.Concurrency
{
    public static class ISchedulerExtensions
    {
        public static IDisposable SafelySchedule(this IScheduler scheduler, Action action)
        {
            return scheduler.Schedule(() =>
            {
                try
                {
                    action();
                }
                catch(Exception){}
            });
        }
    }
}