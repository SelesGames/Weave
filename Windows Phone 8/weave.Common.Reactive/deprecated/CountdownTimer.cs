
namespace System.Windows.Threading
{
    public static class CountdownTimer
    {
        public static CountdownTimerBuilder In(TimeSpan timespan)
        {
            var timer = new DispatcherTimer { Interval = timespan };
            return new CountdownTimerBuilder(timer);
        }

        public class CountdownTimerBuilder : IDisposable
        {
            DispatcherTimer timer;

            public CountdownTimerBuilder(DispatcherTimer timer)
            {
                this.timer = timer;
            }

            public CountdownTimerBuilder Do(Action action)
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    timer.Tick -= handler;
                    action();
                    timer.Stop();
                };
                timer.Tick += handler;
                timer.Start();
                return this;
            }

            public void Stop()
            {
                timer.Stop();
            }

            public void Dispose()
            {
                if (timer != null)
                    timer.Stop();
            }
        }
    }
}