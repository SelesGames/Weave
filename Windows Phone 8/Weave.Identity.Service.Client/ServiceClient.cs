using SelesGames.Rest;
using System;
using System.Threading;
using System.Threading.Tasks;
using Weave.Identity.Service.Contracts;

namespace Weave.Identity.Service.Client
{
    public class ServiceClient : IIdentityService
    {
        const string SERVICE_URL = "http://weave-identity.cloudapp.net/api/identity";

        public async Task<DTOs.IdentityInfo> GetUserFromFacebookToken(string facebookToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("facebookToken", facebookToken)
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<DTOs.IdentityInfo>(url, CancellationToken.None);
            return result;
        }

        public async Task<DTOs.IdentityInfo> GetUserFromTwitterToken(string twitterToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("twitterToken", twitterToken)
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<DTOs.IdentityInfo>(url, CancellationToken.None);
            return result;
        }

        public async Task<DTOs.IdentityInfo> GetUserFromMicrosoftToken(string microsoftToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("microsoftToken", microsoftToken)
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<DTOs.IdentityInfo>(url, CancellationToken.None);
            return result;
        }

        public async Task<DTOs.IdentityInfo> GetUserFromGoogleToken(string googleToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("googleToken", googleToken)
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<DTOs.IdentityInfo>(url, CancellationToken.None);
            return result;
        }

        public async Task<DTOs.IdentityInfo> GetUserFromUserNameAndPassword(string username, string password)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("username", username)
                .AddParameter("password", password)
                .ToString();

            var client = CreateClient();
            var result = await client.GetAsync<DTOs.IdentityInfo>(url, CancellationToken.None);
            return result;
        }

        public Task Add(DTOs.IdentityInfo user)
        {
            var client = CreateClient();
            return client.PostAsync(SERVICE_URL, user, CancellationToken.None);
        }

        public Task Update(DTOs.IdentityInfo user)
        {
            var client = CreateClient();
            return client.PostAsync(SERVICE_URL, user, CancellationToken.None);
        }

        RestClient CreateClient()
        {
            //return new SelesGames.Rest.Protobuf.ProtobufRestClient { UseGzip = true };
            return new SelesGames.Rest.JsonDotNet.JsonDotNetRestClient { UseGzip = true };
        }
    }
}
