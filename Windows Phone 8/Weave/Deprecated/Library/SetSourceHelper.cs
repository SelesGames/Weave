using System;
using System.Collections.Generic;
using System.Windows.Threading;

//namespace weave
//{
    public class SetSourceHelper<T>
    {
        ICollection<T> collection;
        IEnumerator<T> enumerator;
        TimeSpan interval;
        DispatcherTimer timer;
        bool timerStopped = true;

        public SetSourceHelper(ICollection<T> collection, TimeSpan interval)
        {
            this.collection = collection;
            this.interval = interval;
            timer = new DispatcherTimer { Interval = interval };

            timer.Tick += (s, e) =>
            {
                if (enumerator.MoveNext())
                {
                    if (!timerStopped)
                        collection.Add(enumerator.Current);
                }
                else
                {
                    timer.Stop();
                }
            };
        }

        public void SetSource(IEnumerable<T> source)
        {
            if (source == null || collection == null)
                return;

            timerStopped = true;
            timer.Stop();
            if (enumerator != null)
                enumerator.Dispose();
            collection.Clear();

            enumerator = source.GetEnumerator();
            timer.Start();
            timerStopped = false;
        }

        public void PauseSetSource()
        {
            timerStopped = true;
            timer.Stop();
        }
    }

    public static class SetSourceHelper
    {
        public static SetSourceHelper<T> Create<T>(ICollection<T> collection, TimeSpan interval)
        {
            return new SetSourceHelper<T>(collection, interval);
        }
    }
//}
