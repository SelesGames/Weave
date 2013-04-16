using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Weave.ViewModels
{
    public abstract class BaseNewsCollectionViewModel
    {
        List<Guid> feedIds;
        protected IUserCache serviceClient;
        protected Guid userId;

        public ObservableCollection<NewsItem> News { get; private set; }
        public int NewArticleCount { get; set; }
        public int UnreadCount { get; set; }
        public int TotalArticleCount { get; set; }

        public BaseNewsCollectionViewModel()
        {
            this.News = new ObservableCollection<NewsItem>();
        }

        public abstract Task RefreshNews();
    }

    public class NewsCollectionCategoryViewModel : BaseNewsCollectionViewModel
    {
        string category;

        public NewsCollectionCategoryViewModel(string category) 
        {
            this.category = category;
        }

        public override async Task RefreshNews()
        {
            var news = await serviceClient.GetNews(category, true);
            base.News.OrderedDescendingUniqueInsert(news, o => o.LocalDateTime);
        }
    }

    public class NewsCollectionFeedViewModel : BaseNewsCollectionViewModel
    {
        Guid feedId;

        public NewsCollectionFeedViewModel(Guid feedId) 
        {
            this.feedId = feedId;
        }

        public override async Task RefreshNews()
        {
            var news = await serviceClient.GetNews(feedId, true);
            base.News.OrderedDescendingUniqueInsert(news, o => o.LocalDateTime);
        }
    }
}
