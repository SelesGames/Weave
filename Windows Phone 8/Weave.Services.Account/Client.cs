using SelesGames.Rest;
using System;
using System.Threading;
using System.Threading.Tasks;
using Weave.Services.Account.DTOs;

namespace Weave.Services.Account
{
    public class Client
    {
        const string GET_URL = "http://weave-accounts.cloudapp.net/api/user/{0}";
        const string POST_URL = "http://weave-accounts.cloudapp.net/api/user";

        RestClient restClient;

        public Client()
        {
            restClient = new SelesGames.Rest.Protobuf.ProtobufRestClient();
        }

        public Task<UserInfo> GetUser(Guid userId)
        {
            var url = string.Format(GET_URL, userId);
            return restClient.GetAsync<UserInfo>(url, CancellationToken.None);   
        }

        public Task<bool> SaveUser(UserInfo user)
        {
            var url = POST_URL;
            return restClient.PostAsync<UserInfo>(url, user, CancellationToken.None);
        }
    }
}
