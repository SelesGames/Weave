using System;
using System.Net;
using System.Reactive.Linq;
using Hammock;
using Hammock.Authentication.OAuth;

namespace weave.Services.Twitter
{
    public class TwitterRequestToken
    {
        public string OAuthToken { get; set; }
        public string OAuthTokenSecret { get; set; }
        public Uri AuthorizeUrl { get; set; }
    }

    public class OAuthCallBackArgs
    {
        public string OAuthToken { get; set; }
        public string OAuthVerifier { get; set; }

        public bool DoTokensMatch(TwitterRequestToken requestToken)
        {
            return requestToken != null && OAuthToken == requestToken.OAuthToken;
        }
    }

    public static class TwitterAuthService
    {
        public static OAuthCallBackArgs GetCallbackArgs(Uri callbackUri)
        {
            if (callbackUri == null)
                return null;

            var absoluteUri = callbackUri.AbsoluteUri;

            if (absoluteUri.Contains(TwitterSettings.CallbackUri))
            {
                var arguments = absoluteUri.Split('?');

                if (arguments.Length < 1)
                    return null;

                var parameters = arguments[1];
                var requestToken = GetQueryParameter(parameters, "oauth_token");
                var requestVerifier = GetQueryParameter(parameters, "oauth_verifier");
                return new OAuthCallBackArgs
                {
                    OAuthToken = requestToken,
                    OAuthVerifier = requestVerifier,
                };
            }
            else
                return null;
        }




        #region Step 1 - GetRequestToken

        public static IObservable<TwitterRequestToken> GetRequestToken()
        {
            return Observable.Create<TwitterRequestToken>(observer =>
            {
                IDisposable disp = null;
                try
                {
                    var credentials = new OAuthCredentials
                    {
                        Type = OAuthType.RequestToken,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        ConsumerKey = TwitterSettings.ConsumerKey,
                        ConsumerSecret = TwitterSettings.ConsumerKeySecret,
                        Version = TwitterSettings.OAuthVersion,
                        CallbackUrl = TwitterSettings.CallbackUri
                    };

                    var client = new RestClient
                    {
                        Authority = "https://api.twitter.com/oauth",
                        Credentials = credentials,
                        HasElevatedPermissions = true
                    };

                    var request = new RestRequest
                    {
                        Path = "/request_token"
                    };
                    client.BeginRequest(request, new RestCallback(
                        (restRequest, response, userState) =>
                        {
                            try
                            {
                                var oAuthToken = GetQueryParameter(response.Content, "oauth_token");
                                var oAuthTokenSecret = GetQueryParameter(response.Content, "oauth_token_secret");
                                var authorizeUrl = TwitterSettings.AuthorizeUri + "?oauth_token=" + oAuthToken;

                                if (String.IsNullOrEmpty(oAuthToken) || String.IsNullOrEmpty(oAuthTokenSecret))
                                    observer.OnError(new Exception("error calling twitter"));

                                else
                                {
                                    var requestToken = new TwitterRequestToken
                                    {
                                        OAuthToken = oAuthToken,
                                        OAuthTokenSecret = oAuthTokenSecret,
                                        AuthorizeUrl = authorizeUrl.ToUri(),
                                    };
                                    observer.OnNext(requestToken);
                                    observer.OnCompleted();
                                }
                            }
                            catch (Exception exception)
                            {
                                observer.OnError(exception);
                            }
                        }));
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return disp;
            });
        }

        #endregion




        #region Step 2 - GetAccessToken

        public static IObservable<TwitterAccess> GetAccessToken(TwitterRequestToken requestToken, OAuthCallBackArgs callbackArgs)
        {
            return Observable.Create<TwitterAccess>(observer =>
            {
                IDisposable disp = null;
                try
                {
                    var credentials = new OAuthCredentials
                    {
                        Type = OAuthType.AccessToken,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        ConsumerKey = TwitterSettings.ConsumerKey,
                        ConsumerSecret = TwitterSettings.ConsumerKeySecret,
                        Token = requestToken.OAuthToken,
                        TokenSecret = requestToken.OAuthTokenSecret,
                        Verifier = callbackArgs.OAuthVerifier,
                    };

                    var client = new RestClient
                    {
                        Authority = "https://api.twitter.com/oauth",
                        Credentials = credentials,
                        HasElevatedPermissions = true,
                    };

                    var request = new RestRequest
                    {
                        Path = "/access_token",
                    };

                    client.BeginRequest(request, new RestCallback(
                        (restRequest, response, userState) =>
                        {
                            try
                            {
                                var twitteruser = new TwitterAccess
                                {
                                    AccessToken = GetQueryParameter(response.Content, "oauth_token"),
                                    AccessTokenSecret = GetQueryParameter(response.Content, "oauth_token_secret"),
                                    UserId = GetQueryParameter(response.Content, "user_id"),
                                    ScreenName = GetQueryParameter(response.Content, "screen_name")
                                };

                                if (String.IsNullOrEmpty(twitteruser.AccessToken) || String.IsNullOrEmpty(twitteruser.AccessTokenSecret))
                                    observer.OnError(new Exception(response.Content));

                                else
                                {
                                    observer.OnNext(twitteruser);
                                    observer.OnCompleted();
                                }
                            }
                            catch (Exception exception)
                            {
                                observer.OnError(exception);
                            }
                        }));
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return disp;
            });
        }

        #endregion




        #region Helper Methods

        static string GetQueryParameter(string input, string parameterName)
        {
            foreach (string item in input.Split('&'))
            {
                var parts = item.Split('=');
                if (parts[0] == parameterName)
                {
                    return parts[1];
                }
            }
            return String.Empty;
        }

        #endregion
    }
}
