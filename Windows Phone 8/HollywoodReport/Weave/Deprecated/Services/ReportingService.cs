
namespace weave
{
    public static class ReportingService
    {
        public static void ReportCustomCategoryAdded(string categoryName)
        {
            //DebugEx.WriteLine(categoryName);
        }

        public static void ReportCustomFeedAdded(string feedUri)
        {
            //DebugEx.WriteLine(feedUri);
        }

        public static void ReportCategoryNavigatedTo(string categoryName)
        {
            //if (string.IsNullOrEmpty(categoryName))
            //    categoryName = "All News";
            //DebugEx.WriteLine(categoryName);
        }
    }
}
