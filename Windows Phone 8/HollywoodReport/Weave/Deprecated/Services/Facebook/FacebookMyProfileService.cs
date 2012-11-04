using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization.Json;

namespace weave.Services.Facebook
{
    public class MeJson
    {
        public string id { get; set; }
    }

    public static class FacebookMyProfileService
    {
        public static IObservable<MeJson> GetMyInfo(FacebookAccess token)
        {
            return Observable.Create<MeJson>(observer =>
            {
                var disp = Disposable.Empty;
                try
                {
                    string url = string.Format(
                        "https://graph.facebook.com/me?access_token={0}",
                        HttpUtility.UrlEncode(token.AccessToken));

                    var request = url.ToUri().ToWebRequest();

                    disp = request.GetWebResponseAsync()
                        .Subscribe(
                            response =>
                            {
                                try
                                {
                                    var serializer = new DataContractJsonSerializer(typeof(MeJson));
                                    var stream = response.GetResponseStream();
                                    var myInfo = (MeJson)serializer.ReadObject(stream);
                                    observer.OnNext(myInfo);
                                    observer.OnCompleted();
                                }
                                catch (Exception exception)
                                {
                                    observer.OnError(exception);
                                }
                            },
                            observer.OnError);
                }
                catch(Exception exception)
                {
                    observer.OnError(exception);
                }
                return disp;
            });
        }
    }
}
