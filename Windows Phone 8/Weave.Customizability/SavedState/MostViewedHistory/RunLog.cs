using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.Customizability.SavedState.MostViewedHistory
{
    public class RunLog
    {
        public List<LabelTally> Log { get; set; }

        public RunLog()
        {
            Log = new List<LabelTally>();
        }

        public void Tally(string label)
        {
            if (string.IsNullOrEmpty(label))
                return;

            var entry = Log.FirstOrDefault(o => label.Equals(o.Label, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                entry = new LabelTally { Label = label };
                Log.Add(entry);
            }

            entry.Count++;
        }
    }
}
