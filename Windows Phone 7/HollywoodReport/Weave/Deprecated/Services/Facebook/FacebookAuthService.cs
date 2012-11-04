using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace weave.Services.Facebook
{
    public static class FacebookAuthService
    {
        static readonly string redirectUri = "http://www.facebook.com/connect/login_success.html";

        public static string GetLoginPageUrl()
        {
            string unformatted =
"http://m.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&scope=offline_access,publish_stream";

            
            
//            string unformatted =
//"https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&display=touch&scope=offline_access,publish_stream,user_status,friends_status";

            return string.Format(
                unformatted,
                FacebookSettings.AppId,
                redirectUri);
        }

        static string GetAccessTokenUrl(string code)
        {
            string unformatted =
"https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";

            return string.Format(
                unformatted,
                FacebookSettings.AppId,
                redirectUri,
                FacebookSettings.AppSecret,
                code);
        }

        public static IObservable<FacebookAccess> GetAccessTokenAsync(string code)
        {
            return Observable.Create<FacebookAccess>(observer =>
            {
                var disp = Disposable.Empty;
                try
                {
                    disp = GetAccessTokenUrl(code).ToUri().ToWebRequest().GetWebResponseAsync()
                        .Subscribe(
                            response =>
                            {
                                try
                                {
                                    var result = response.GetResponseStreamAsString();
                                    var fullAccessToken = GetQueryParameter(result, "access_token");
                                    var facebookAccess = new FacebookAccess
                                    {
                                        AccessToken = fullAccessToken,
                                    };
                                    observer.OnNext(facebookAccess);
                                    observer.OnCompleted();
                                }
                                catch (Exception exception)
                                {
                                    observer.OnError(exception);
                                }
                            },
                            exception => observer.OnError(exception));
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return disp;
            });
        }

        public static string GetCode(Uri callbackUri)
        {
            if (callbackUri == null)
                return null;

            var absoluteUri = callbackUri.AbsoluteUri;

            if (absoluteUri.StartsWith(redirectUri))
            {
                var arguments = absoluteUri.Split('?');

                if (arguments.Length < 1)
                    return null;

                var parameters = arguments[1];
                var code = GetQueryParameter(parameters, "code");
                return code;
            }
            else
                return null;
        }



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

        public static bool GetIsAtPermissionsPage(Uri uri)
        {
            var absUri = uri.AbsoluteUri;

            return absUri.StartsWith("http://www.facebook.com/connect/uiserver.php?") &&
                absUri.Contains("method=permissions.request");
        }

        public static bool GetIsVerifiedAtPermissionsPage(string html)
        {
            return html.IndexOf("<TITLE>Request for Permission</TITLE>", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
