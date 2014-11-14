using System.Collections.Generic;
using System.Linq;

namespace Weave.SavedState.MostViewedHistory
{
    public class RunHistory
    {
        readonly int logLimit = 10;
        RunLog activeLog;
        public List<RunLog> History { get; set; }

        public RunHistory()
        {
            History = new List<RunLog>();
        }

        public void CreateNewLog()
        {
            var log = new RunLog();
            History.Add(log);
            activeLog = log;
            if (History.Count > logLimit)
                History.RemoveAt(0);
        }

        public RunLog GetActiveLog()
        {
            EnsureInit();
            return activeLog;
        }

        public IEnumerable<LabelTally> GetTallies()
        {
            return History
                .SelectMany(o => o.Log)
                .GroupBy(o => o.Label)
                .Select(o => new LabelTally { Label = o.Key, Count = o.Aggregate(0, (seed, x) => seed += x.Count) })
                .OrderByDescending(o => o.Count)
                .ToList();
        }

        void EnsureInit()
        {
            if (activeLog == null)
                activeLog = History.LastOrDefault();
            if (activeLog == null)
            {
                activeLog = new RunLog();
                History.Add(activeLog);
            }
        }
    }
}
