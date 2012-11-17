
namespace weave.Services.MostViewedHistory
{
    public class LabelTally
    {
        public string Label { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Label, Count);
        }
    }
}
