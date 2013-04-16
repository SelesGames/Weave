using System;
using Weave.UserFeedAggregator.DTOs.ServerOutgoing;

namespace Weave.UserFeedAggregator.Contracts
{
    public interface IUserInfoCache
    {
        UserInfo Get(Guid userId);
    }
}
