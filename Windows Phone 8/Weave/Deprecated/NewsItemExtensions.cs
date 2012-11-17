
namespace weave
{
    public static class NewsItemExtensions
    {
        public static StarredNewsItem ToStarredNewsItem(this NewsItem newsItem)
        {
            return new StarredNewsItem
            {
                //Category = newsItem.FeedSource.Category,
                Title = newsItem.Title,
                //Description = newsItem.Description,
                Link = newsItem.Link,
                ImageUrl = newsItem.ImageUrl,
                PublishDateTime = newsItem.PublishDateTime,
            };
        }
    }
}
