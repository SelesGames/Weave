using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace weave
{
    public class ImageCache
    {
        IDictionary<string, BitmapImage> cache;
        object syncHandle = new object();
        HttpWebRequestQueue requestQueue = new HttpWebRequestQueue(5);
        //ProcessQueue<Tuple<HttpWebResponse, BitmapImage>, Unit> imageSetterQueue;

        public ImageCache()
        {
            cache = new Dictionary<string, BitmapImage>();
            //imageSetterQueue = new ProcessQueue<Tuple<HttpWebResponse, BitmapImage>, Unit>(1,
            //    t =>
            //    {
            //        var response = t.Item1;
            //        var imageSource = t.Item2;

            //        return Observable.Create<Unit>(observer =>
            //        {
            //            DebugEx.WriteLine("TESTTESTSETSETSET");
            //            GlobalDispatcher.Current.BeginInvoke(() =>
            //            {
            //                try
            //                {
            //                    using (var stream = response.GetResponseStream())
            //                    {
            //                        //IsolatedStorageService.SaveImage(i, url);
            //                        imageSource.SetSource(stream);
            //                        stream.Close();
            //                    }
            //                    observer.OnNext(new Unit());
            //                    observer.OnCompleted();
            //                }
            //                catch (Exception ex)
            //                {
            //                    DebugEx.WriteLine("Exception setting image source: {0}", ex);
            //                    observer.OnError(ex);
            //                }
            //                finally
            //                {
            //                    response.Close();
            //                    response.Dispose();
            //                }
            //            });
            //            return () => { };
            //        });
            //    });
        }

        public void Flush()
        {
            requestQueue.Flush();
            if (cache.Count == 0)
                return;

            DebugEx.WriteLine("Flushing image cache");
            lock (syncHandle)
            {
                foreach (var image in cache.Values)
                {
                    var capturedImage = image;
                    GlobalDispatcher.Current.BeginInvoke(() =>
                    {
                        capturedImage.UriSource = null;
                        //capturedImage.SetSource(null);
                        capturedImage = null;
                    });
                }
                cache.Clear();
            }
            //cache = new Dictionary<string, BitmapImage>();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); 
        }

        public ImageSource GetImage(string url)
        {
            ImageSource existingImageSource = null;
            lock (syncHandle)
            {
                existingImageSource = cache.GetValueOrDefaultForKey(url, null);
            }
            //var stream = IsolatedStorageService.GetImage(url);
            //if (stream != null)
            //{
            //var cachedImage = IsolatedStorageService.GetImage(url);// new BitmapImage();
            //if (cachedImage != null)
            //{
            //    DebugEx.WriteLine("Successfully recovered image {0} from cache", url);
            //    return cachedImage;
            //}
            //cachedImage.SetSource(stream);
            //stream.Close();
            //stream.Dispose();
            //return cachedImage;
            //}
            //existingImageSource = new BitmapImage();
            //existin.SetSource
            if (existingImageSource is ImageSource)
                return (ImageSource)existingImageSource;

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var x = new BitmapImage();

                lock (syncHandle)
                {
                    cache.UpdateValueForKey(x, url);
                }

                IDisposable disposeHandle = null;
                try
                {
                    var request = url.ToUri().ToWebRequest() as HttpWebRequest;
                    if (request == null)
                        return null;

                    disposeHandle = request.GetWebResponseAsync()
                        .ObserveOnDispatcher()
                        .Subscribe(i =>
                        {
                            disposeHandle.Dispose();

                            try
                            {
                                using (var stream = i.GetResponseStream())
                                {
                                    //IsolatedStorageService.SaveImage(i, url);
                                    x.SetSource(stream);
                                    stream.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                DebugEx.WriteLine("Exception setting image source: {0}", ex);
                            }
                            finally
                            {
                                i.Close();
                                i.Dispose();
                            }
                        },
                        (ex) =>
                        {
                            DebugEx.WriteLine("Exception in loading image {0}", url);
                            disposeHandle.Dispose();
                        });
                    return x;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
                return null;
        }

        public Tuple<ImageSource, IObservable<Unit>> GetImageWithNotification(string url)
        {
            ImageSource existingImageSource = null;
            lock (syncHandle)
            {
                existingImageSource = cache.GetValueOrDefaultForKey(url, null);
            }
            //var stream = IsolatedStorageService.GetImage(url);
            //if (stream != null)
            //{
            //var cachedImage = IsolatedStorageService.GetImage(url);// new BitmapImage();
            //if (cachedImage != null)
            //{
            //    DebugEx.WriteLine("Successfully recovered image {0} from cache", url);
            //    return cachedImage;
            //}
            //cachedImage.SetSource(stream);
            //stream.Close();
            //stream.Dispose();
            //return cachedImage;
            //}
            //existingImageSource = new BitmapImage();
            //existin.SetSource
            if (existingImageSource is ImageSource)
                return Tuple.Create((ImageSource)existingImageSource, new[] { new Unit() }.ToObservable());

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var x = new BitmapImage();

                lock (syncHandle)
                {
                    cache.UpdateValueForKey(x, url);
                }

                IDisposable disposeHandle = null;
                try
                {
                    var request = url.ToUri().ToWebRequest() as HttpWebRequest;
                    if (request == null)
                        return null;

                    request.AllowReadStreamBuffering = true; // try this out

                    var notifier = new Subject<Unit>();
                    disposeHandle = this.requestQueue.Enqueue(request)//.GetWebResponseAsync()
                        .ObserveOnDispatcher()
                        .Subscribe(i =>
                        {
                            disposeHandle.Dispose();

                            //imageSetterQueue
                            //    .Enqueue(Tuple.Create(i, x))
                            //    .Subscribe(
                            //        unit =>
                            //        {
                            //            notifier.OnNext(unit);
                            //            notifier.OnCompleted();
                            //        },
                            //        ex => notifier.OnError(ex));

                            try
                            {
                                using (var stream = i.GetResponseStream())
                                {
                                    //IsolatedStorageService.SaveImage(i, url);
                                    x.SetSource(stream);
                                    stream.Close();
                                }
                                notifier.OnNext(new Unit());
                                notifier.OnCompleted();
                            }
                            catch (Exception ex)
                            {
                                notifier.OnError(ex);
                                DebugEx.WriteLine("Exception setting image source: {0}", ex);
                            }
                            finally
                            {
                                i.Close();
                                i.Dispose();
                            }
                        },
                        (ex) =>
                        {
                            notifier.OnError(ex);
                            DebugEx.WriteLine("Exception in loading image {0}", url);
                            disposeHandle.Dispose();
                        });
                    return Tuple.Create((ImageSource)x, notifier.AsObservable().Take(1));
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
                return null;
        }
    }
}
