using System.Disposables;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Net
{
    public static class WebRequestExtensionMethods
    {


        #region Helper functions for creating Uri from string and WebRequest from Uri

        public static Uri ToUri(this string uri, UriKind uriKind = UriKind.Absolute)
        {
            if (!string.IsNullOrEmpty(uri) && Uri.IsWellFormedUriString(uri, uriKind))
                return new Uri(uri, uriKind);
            else
                return null;
        }

        public static WebRequest ToWebRequest(this Uri uri)
        {
            if (uri == null)
                return null;

            WebRequest request = null;
            try
            {
                //request = WebRequestCreator.ClientHttp.Create(uri);
                request = HttpWebRequest.Create(uri);
            }
            catch (Exception)
            {
            }
            return request;
        }

        #endregion



        #region Helper functions converting WebResponse and Stream to a string value

        public static string GetResponseStreamAsString(this WebResponse response)
        {
            string result = null;
            using (var stream = response.GetResponseStream())
            {
                result = stream.GetReadStream();
                stream.Close();
            }
            return result;
        }

        public static string GetReadStream(this Stream stream)
        {
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    reader.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("Exception in GetReadStream: \r\n{0}", ex);
                return null;
            }
        }

        #endregion



        #region GETWEBRESPONSEASYNC - translates a request into an IObservable WebResponse

        public static IObservable<WebResponse> GetWebResponseAsync(this WebRequest request)
        {
            return Observable.CreateWithDisposable<WebResponse>(observer =>
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

        #endregion



        #region GetRequestStream, WriteToRequestStream

        public static IObservable<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            return Observable.CreateWithDisposable<Stream>(observer =>
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
            return Observable.CreateWithDisposable<Unit>(observer =>
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
            return Observable.CreateWithDisposable<WebResponse>(observer =>
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

        public static IObservable<HttpWebResponse> GetHttpWebResponseAsync(this HttpWebRequest request)
        {
            return request
                .GetWebResponseAsync()
                .Where(response => response is HttpWebResponse)
                .Select(response => (HttpWebResponse)response);
        }

        public static IObservable<WebResponse> GetWebResponseAsync(this string url)
        {
            return Observable.CreateWithDisposable<WebResponse>(observer =>
            {
                var disp = Disposable.Empty;
                try
                {
                    var request = url.ToUri().ToWebRequest() as HttpWebRequest;
                    request.AllowAutoRedirect = false;

                    disp = request.GetWebResponseAsync()
                        .Subscribe(
                            response =>
                            {
                                try
                                {
                                    observer.OnNext(response);
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

        public static IObservable<WebResponse> GetWebResponseAsync(this WebRequest request, string postData)
        {
            return Observable.CreateWithDisposable<WebResponse>(observer =>
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