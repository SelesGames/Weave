using System;
using System.Net;
using System.Reactive.Linq;

namespace weave.Services.Facebook
{
    public static class FacebookMyPicService
    {
        public static IObservable<string> GetMyPicUrl(string userId)
        {
            string url = string.Format(
                "http://graph.facebook.com/{0}/picture?type=square",
                HttpUtility.UrlEncode(userId));
            return url.GetWebResponseAsync().Select(response =>
            {
                var transport = response.Headers["Location"];
                DebugEx.WriteLine(transport);
                return transport;
            });
            //return Observable.CreateWithDisposable<string>(observer =>
            //{
            //    var disp = Disposable.Empty;
            //    try
            //    {
            //        string url = string.Format(
            //            "http://graph.facebook.com/{0}/picture",
            //            HttpUtility.UrlEncode(userId));

            //        var request = url.ToUri().ToWebRequest();

            //        disp = request.GetWebResponseAsync()
            //            .Subscribe(
            //                response =>
            //                {
            //                    try
            //                    {
            //                        var picUrl = response.GetResponseStreamAsString();
            //                        observer.OnNext(picUrl);
            //                        observer.OnCompleted();
            //                    }
            //                    catch (Exception exception)
            //                    {
            //                        observer.OnError(exception);
            //                    }
            //                },
            //                exception => observer.OnError(exception));
            //    }
            //    catch (Exception exception)
            //    {
            //        observer.OnError(exception);
            //    }
            //    return disp;
            //});
        }
    }
}
