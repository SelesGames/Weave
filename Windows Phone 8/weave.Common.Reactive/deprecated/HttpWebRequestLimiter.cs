using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace System.Net
{
    public class HttpWebRequestLimiter
    {
        List<HttpWebRequest> requestQueue = new List<HttpWebRequest>();
        int maxOutstandingRequests = 8;

        public int MaxOutstandingRequests
        {
            get { return maxOutstandingRequests; }
            set { maxOutstandingRequests = value; } // would have to flush excess requests here
        }

        public async Task<HttpWebResponse> GetResponseAsync(HttpWebRequest request)
        {
            requestQueue.Add(request);
            //DebugEx.WriteLine("adding {0}", request.RequestUri.OriginalString);
            if (requestQueue.Count > maxOutstandingRequests && requestQueue.Any())
            {
                var cancel = requestQueue.First();
                requestQueue.Remove(cancel);

                try
                {
                    cancel.Abort();
                }
                catch { }
            }

            try
            {
                var response = await request.GetWebResponseFullyAsync().ToTask();
                return (HttpWebResponse)response;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                requestQueue.Remove(request);
            }
        }

        public IObservable<HttpWebResponse> GetResponseAsyncObservable(HttpWebRequest request)
        {
            return Observable.Create<HttpWebResponse>(observer =>
            {
                try
                {
                    requestQueue.Add(request);
                    //DebugEx.WriteLine("adding {0}", request.RequestUri.OriginalString);
                    if (requestQueue.Count > maxOutstandingRequests && requestQueue.Any())
                    {
                        var cancel = requestQueue.First();
                        requestQueue.Remove(cancel);

                        try
                        {
                            cancel.Abort();
                        }
                        catch { }
                    }

                    return request.GetWebResponseFullyAsync().Select(o => (HttpWebResponse)o)
                        .Finally(() => requestQueue.Remove(request))
                        .Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    return Disposable.Empty;
                }
            });
        }
    }
}
