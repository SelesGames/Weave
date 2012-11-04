using System;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Web;

namespace weave.Services.Twitter
{
    public static class TwitterPostService
    {
        public static IObservable<Unit> PostTweet(string message, TwitterAccess accessToken)
        {
            return Observable.Create<Unit>(observer =>
            {
                IDisposable disp = Disposable.Empty;
                if (message.Length > 200)
                {
                    observer.OnError(new Exception("tweet is greater than 200 characters"));
                    return disp;
                }
                if (accessToken == null)
                {
                    observer.OnError(new Exception("accessToken was null"));
                    return disp;
                }

                try
                {
                    var credentials = new OAuthCredentials
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

                    var client = new RestClient
                    {
                        Authority = "http://api.twitter.com",
                        HasElevatedPermissions = true
                    };

                    var request = new RestRequest
                    {
                        Credentials = credentials,
                        Path = "/statuses/update.xml",
                        Method = WebMethod.Post
                    };

                    request.AddParameter("status", message);

                    client.BeginRequest(request, new RestCallback(
                        (restRequest, response, userState) =>
                        {
                            try
                            {
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    observer.OnNext(new Unit());
                                    observer.OnCompleted();
                                }
                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    observer.OnError(new UnauthorizedCredentialsException());
                                }
                                else
                                {
                                    observer.OnError(new Exception(response.StatusCode.ToString()));
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
    }

    public class UnauthorizedCredentialsException : Exception
    {
    }
}
