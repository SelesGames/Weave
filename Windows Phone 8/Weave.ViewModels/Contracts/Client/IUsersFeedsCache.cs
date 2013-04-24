using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weave.ViewModels.Contracts.Client
{
    public interface IUsersFeedsCache
    {
        Task<IEnumerable<Feed>> Get();
        Task Add(Feed feed);
        Task Remove(Feed feed);
        Task Update(Feed feed);
        Task BatchChange(IEnumerable<Feed> added, IEnumerable<Feed> removed);
    }
}
