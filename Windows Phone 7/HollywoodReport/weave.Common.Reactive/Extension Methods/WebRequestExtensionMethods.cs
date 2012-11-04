using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace System.Net
{
    public static class WebRequestExtensionMethods
    {
        #region GETWEBRESPONSEASYNC - translates a request into an IObservable WebResponse

        public static IObservable<WebResponse> GetWebResponseAsync(this WebRequest request)
        {
            return Observable.Create<WebResponse>(observer =>
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
                            var response = request.EndGetResponse(result);
                            observer.OnNext(response);
                            observer.OnCompleted();
                            //DebugEx.WriteLine("COMPLETED WebRequest.GetResponseAsync ({0})", request.RequestUri);
                        }
                        catch (WebException ex)
                        {
                            if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotModified)
                            {
                                observer.OnNext(ex.Response);
                                observer.OnCompleted();
                            }
                            else if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden)
                            {
                                observer.OnNext(ex.Response);
                                observer.OnCompleted();
                            }
                            else if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                            {
                                observer.OnNext(ex.Response);
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

        public static IObservable<WebResponse> GetWebResponseFullyAsync(this WebRequest request)
        {
            return GetWebResponseFullyAsync(request, Scheduler.ThreadPool);
        }

        public static IObservable<WebResponse> GetWebResponseFullyAsync(this WebRequest request, IScheduler scheduler)
        {
            return Observable.Create<WebResponse>(observer =>
            {
                var disposables = new CompositeDisposable();
                try
                {
                    scheduler.Schedule(() =>
                    {
                        try
                        {
                            Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse)()
                                .Subscribe(observer.OnNext, ex => HandleWebResponseException(observer, ex), observer.OnCompleted)
                                .DisposeWith(disposables);
                        }
                        catch(Exception e) { observer.OnError(e); }
                    }).DisposeWith(disposables);
                }
                catch(Exception e) { observer.OnError(e); }
                return disposables;
            });
        }

        static void HandleWebResponseException(IObserver<WebResponse> observer, Exception e)
        {
            if (e is WebException)
            {
                var ex = (WebException)e;

                if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotModified)
                {
                    observer.OnNext(ex.Response);
                    observer.OnCompleted();
                }
                else if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden)
                {
                    observer.OnNext(ex.Response);
                    observer.OnCompleted();
                }
                else if (ex.Response is HttpWebResponse && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    observer.OnNext(ex.Response);
                    observer.OnCompleted();
                }
                else
                {
                    observer.OnError(ex);
                }
            }
            else
            {
                observer.OnError(e);
            }
        }
        
        #endregion



        #region GetRequestStream, WriteToRequestStream

        public static IObservable<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            return Observable.Create<Stream>(observer =>
            {
                IDisposable disp = Disposable.Empty;
                try
                {
                    IAsyncResult result = null;
                    result = request.BeginGetRequestStream(notUsed =>
                    {
                        try
                        {
                            var response = request.EndGetRequestStream(result);
                            observer.OnNext(response);
                            observer.OnCompleted();
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(new Exception("Error with the anonymous callback in BeginGetRequestStream inside GetRequestStreamAsync", ex));
                        }
                    }, null);
                }
                catch (Exception ex) { observer.OnError(ex); }
                return disp;
            });  
        }

        public static IObservable<Unit> WriteToRequestStreamAsync(this WebRequest request, object graph, Action<Stream, object> writeAction)
        {
            return Observable.Create<Unit>(observer =>
            {
                return
                    request.GetRequestStreamAsync().Subscribe(
                        stream => 
                        {
                            try
                            {
                                writeAction(stream, graph);
                                stream.Close();
                                stream.Dispose();
                                observer.OnNext(new Unit());
                                observer.OnCompleted();
                            }
                            catch (Exception exception)
                            {
                                observer.OnError(exception);
                            }
                        },
                        observer.OnError);
            });
        }

        #endregion



        #region Specialized versions of GetWebResponseAsync (support for Timeout, from string, Post data, HttpWebResponse)

        public static IObservable<WebResponse> GetWebResponseAsyncWithTimeout(this WebRequest request, TimeSpan timeout)
        {
            return Observable.Create<WebResponse>(observer =>
            {
                var disposable = Disposable.Empty;
                try
                {
                    disposable = request.GetWebResponseAsync().Timeout(timeout).Subscribe(
                        response =>
                        {
                            observer.OnNext(response);
                            observer.OnCompleted();
                        },
                        exception =>
                        {
                            if (exception is TimeoutException)
                            {
                                try
                                {
                                    request.Abort();
                                }
                                catch (Exception) { }
                            }
                            observer.OnError(exception);
                        },
                        observer.OnCompleted);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return disposable;
            });
        }

        public static IObservable<HttpWebResponse> GetHttpWebResponseAsync(this WebRequest request)
        {
            return request
                .GetWebResponseAsync()
                .Where(response => response is HttpWebResponse)
                .Select(response => (HttpWebResponse)response);
        }

        public static IObservable<HttpWebResponse> GetHttpWebResponseAsync(this string url)
        {
            return Observable.Create<HttpWebResponse>(observer =>
            {
                var disp = Disposable.Empty;
                try
                {
                    disp = HttpWebRequest.Create(url).GetHttpWebResponseAsync()
                        .Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return disp;
            });
        }

        public static IObservable<WebResponse> GetWebResponseAsync(this WebRequest request, string postData)
        {
            return Observable.Create<WebResponse>(observer =>
            {
                var disposables = new CompositeDisposable();
                try
                {
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";

                    var requestStream = Observable.FromAsyncPattern<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream);
                    IDisposable disp;
                    disp = requestStream()
                        .Subscribe(
                            stream =>
                            {
                                try
                                {
                                    byte[] data = Encoding.UTF8.GetBytes(postData);
                                    stream.Write(data, 0, data.Length);
                                    stream.Close();

                                    disposables.Add(stream);

                                    disp = request.GetWebResponseAsync()
                                        .Subscribe(
                                            response =>
                                            {
                                                observer.OnNext(response);
                                                observer.OnCompleted();
                                            },
                                            exception => observer.OnError(exception));

                                    disposables.Add(disp);
                                }
                                catch (Exception exception)
                                {
                                    observer.OnError(exception);
                                }
                            },
                            exception => observer.OnError(exception));

                    disposables.Add(disp);
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                }
                return disposables;
            });
        }
        #endregion
    }
}