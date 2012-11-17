using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace System.Net
{
    public static class WebRequestExtensionMethods
    {
        public static IObservable<HttpWebResponse> GetHttpWebResponseAsync(this HttpWebRequest request)
        {
            return Observable.Create<HttpWebResponse>(observer =>
            {
                IDisposable disp = Disposable.Empty;
                try
                {
                    //DebugEx.WriteLine("STARTED WebRequest.GetResponseAsync ({0})", request.RequestUri);
                    IAsyncResult result = null;
                    result = request.BeginGetResponse(notUsed =>
                    {
                        try
                        {
                            var response = (HttpWebResponse)request.EndGetResponse(result);
                            observer.OnNext(response);
                            observer.OnCompleted();
                            //DebugEx.WriteLine("COMPLETED WebRequest.GetResponseAsync ({0})", request.RequestUri);
                        }
                        catch (WebException ex)
                        {
                            if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotModified)
                            {
                                observer.OnNext((HttpWebResponse)ex.Response);
                                observer.OnCompleted();
                            }
                            else if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden)
                            {
                                observer.OnNext((HttpWebResponse)ex.Response);
                                observer.OnCompleted();
                            }
                            else if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                            {
                                observer.OnNext((HttpWebResponse)ex.Response);
                                observer.OnCompleted();
                            }
                            else
                            {
                                observer.OnError(ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(new Exception("Error with the anonymous callback in BeginGetResponse inside GetWebResponseAsync", ex));
                        }
                    }, null);
                }
                catch (Exception ex) { observer.OnError(ex); }
                return disp;
            })
            .Take(1);
        }
    }
}