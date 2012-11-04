
namespace weave
{
    public static class NewsItemExtensions
    {
        public static bool IsNew(this NewsItem newsItem)
        {
            return !newsItem.HasBeenViewed && newsItem.OriginalDownloadDateTime > AppSettings.Instance.LastLoginTime;
        }
    }
}
