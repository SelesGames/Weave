using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Authentication;

namespace weave.Services.Twitter
{
    public class TwitterClient
    {
        string consumerKey, consumerSecret;
        TwitterAccess accessToken;

        public RestClient Client { get; private set; }
        public IWebCredentials Credentials { get; private set; }


        public TwitterClient(string consumerKey, string consumerSecret, TwitterAccess accessToken)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.accessToken = accessToken;

            Credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = TwitterSettings.ConsumerKey,
                ConsumerSecret = TwitterSettings.ConsumerKeySecret,
                Token = accessToken.AccessToken,
                TokenSecret = accessToken.AccessTokenSecret,
                Version = TwitterSettings.OAuthVersion,
            };

            Client = new RestClient
            {
                Authority = "http://api.twitter.com",
                HasElevatedPermissions = true
            };
        }
    }
}
