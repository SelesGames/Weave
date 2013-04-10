using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;

namespace Weave.UserFeedAggregator.Contracts
{
    public interface IServiceClient
    {
        Task<Outgoing.UserInfo> AddUserAndReturnNewNews(Incoming.UserInfo incomingUser);
        //Task<Outgoing.UserInfo> GetUserInfoWithNoNews(Guid userId);
        //Task<Outgoing.UserInfo> GetUserInfoWithRefreshedNewsCount(Guid userId);
        Task<Outgoing.UserInfo> RefreshAndReturnNews(Guid userId);
        Task<Outgoing.UserInfo> RefreshAndReturnNews(Guid userId, List<Guid> feedIds);

        Task AddFeed(Guid userId, Incoming.Feed feed);
        Task RemoveFeed(Guid userId, Guid feedId);
        Task UpdateFeed(Guid userId, Incoming.Feed feed);

        Task MarkArticleRead(Guid userId, Guid feedId, Guid newsItemId);
        Task MarkArticleUnread(Guid userId, Guid feedId, Guid newsItemId);
    }
}
