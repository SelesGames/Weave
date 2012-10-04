
namespace ZuneCrawler.Core.Zest
{
    internal static class ZestCategoryExtensionMethods
    {
        internal static Category ToCategory(this ZestCategory category)
        {
            return new Category
            {
                Id = category.Id,
                IsRoot = category.IsRoot,
                Title = category.Title,
            };
        }
    }
}
