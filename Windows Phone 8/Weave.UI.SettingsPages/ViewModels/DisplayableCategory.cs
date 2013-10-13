
namespace weave
{
    public class DisplayableCategory
    {
        public static DisplayableCategory NONE = new DisplayableCategory { Name = null, DisplayName = "None" };

        public string Name { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
