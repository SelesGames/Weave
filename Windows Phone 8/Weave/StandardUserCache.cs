using System;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;

namespace Weave.ViewModels.Cache
{
    public class StandardUserCache : IUserCache
    {
        UserInfo user;
        IViewModelRepository viewModelRepo;

        public StandardUserCache(IViewModelRepository viewModelRepo)
        {
            this.viewModelRepo = viewModelRepo;
        }

        public async Task RefreshUser(bool refreshNews = false)
        {
            user = await viewModelRepo.GetUserInfo(refreshNews);
        }

        public UserInfo Get()
        {
            EnsureInitialUserDownload();
            return user;
        }

        void EnsureInitialUserDownload()
        {
            if (user == null)
                throw new Exception("user not set in StandardUserCache");
        }
    }
}
