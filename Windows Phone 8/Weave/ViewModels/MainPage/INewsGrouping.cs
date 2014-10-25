using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.WP.ViewModels.GroupedNews;

namespace Weave.WP.ViewModels.MainPage
{
    public interface INewsGrouping
    {
        IPagedNews CreatePage(int pageSize, int numberOfPagesToTakeAtATime);
    }

    public class ArticleGroupingAdapter : INewsGrouping
    {
        ArticleGroup group;

        public ArticleGroupingAdapter(ArticleGroup group)
        {
            this.group = group;
        }

        public IPagedNews CreatePage(int pageSize, int numberOfPagesToTakeAtATime)
        {
            return new PagedArticles(group, pageSize, numberOfPagesToTakeAtATime);
        }
    }

    public class NewsGroupingAdapter : INewsGrouping
    {
        NewsItemGroup group;

        public NewsGroupingAdapter(NewsItemGroup group)
        {
            this.group = group;
        }

        public IPagedNews CreatePage(int pageSize, int numberOfPagesToTakeAtATime)
        {
            return new PagedNewsItems(group, pageSize, numberOfPagesToTakeAtATime);
        }
    }
}
