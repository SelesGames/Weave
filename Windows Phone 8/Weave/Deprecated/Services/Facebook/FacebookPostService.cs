using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace weave.Services.Facebook
{
    public static class FacebookPostService
    {
        public static IObservable<WebResponse> PostToWall(FacebookWallPost wallPost, FacebookAccess token)
        {
            return Observable.Create<WebResponse>(observer =>
            {
                var disp = Disposable.Empty;
                try
                {
                    string url = string.Format(
                        "https://graph.facebook.com/me/feed?access_token={0}",
                        HttpUtility.UrlEncode(token.AccessToken));

                    var request = url.ToUri().ToWebRequest();

                    var postParameters = wallPost.GetPostParameters();
                    disp = request.GetWebResponseAsync(postParameters)
                        .Subscribe(
                            response =>
                            {
                                var webResponse = response as HttpWebResponse;

                                if (webResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    observer.OnNext(response);
                                    observer.OnCompleted();
                                }
                                else
                                    observer.OnError(new UnauthorizedCredentialsException());
                            },
                            exception => 
                            {
                                if (exception != null &&
                                    exception.InnerException != null &&
                                    exception.InnerException.Message == "The remote server returned an error: NotFound.")
                                    observer.OnError(new UnauthorizedCredentialsException());
                                else
                                    observer.OnError(exception);
                            });
                }
                catch(Exception exception)
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
