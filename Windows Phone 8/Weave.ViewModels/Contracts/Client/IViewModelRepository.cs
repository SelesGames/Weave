using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.ViewModels.Contracts.Client
{
    public interface IViewModelRepository
    {
        //Task<UserInfo> AddUserAndReturnUserInfo(UserInfo incomingUser);
        //Task<UserInfo> GetUserInfo(Guid userId, bool refresh = false);
        Task<IList<NewsItem>> GetNews(string category, bool refresh = false, int skip = 0, int take = 10);
        Task<IList<NewsItem>> GetNews(Guid feedId, bool refresh = false, int skip = 0, int take = 10);

        Task<IList<NewsItem>> GetFeaturedNews(string category, int take, bool refresh = false);
        Task<IList<NewsItem>> GetFeaturedNews(Guid feedId, int take, bool refresh = false);

        Task AddFeed(Feed feed);
        Task RemoveFeed(Feed feed);
        Task UpdateFeed(Feed feed);
        
        Task MarkArticleRead(NewsItem newsItem);
        Task MarkArticleUnread(NewsItem newsItem);
    }
}
