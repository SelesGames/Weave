using System;
using System.Threading.Tasks;
using Weave.ViewModels.Contracts.Client;

namespace Weave.ViewModels.Cache
{
    public class StandardUserCache : IUserCache
    {
        UserInfo user;

        public StandardUserCache(UserInfo user)
        {
            this.user = user;
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
