using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reactive;
using System.Windows;

namespace weave
{
    public static class IEnumerableKeyValuePairExtensions
    {
        public static bool Contains<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> coll, TKey key)
        {
            if (coll == null) throw new ArgumentNullException("coll");
            if (key == null) throw new ArgumentNullException("key");

            return coll.Where(o => o.Key != null).Any(o => o.Key.Equals(key));
        }

        public static Tuple<bool, TValue> TryGet<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> coll, TKey key)
        {
            if (coll == null) throw new ArgumentNullException("coll");
            if (key == null) throw new ArgumentNullException("key");

            var matches = coll.Where(o => o.Key != null).Where(o => o.Key.Equals(key));

            if (matches.Any())
                return Tuple.Create(true, matches.First().Value);
            else
                return Tuple.Create(false, default(TValue));
        }
    }

    public class ImageCache // : IDisposable
    {
        object syncHandle = new object();
        HttpWebRequestQueue requestQueue = new HttpWebRequestQueue(4, DispatcherScheduler.Instance, Schedulers.ImageDownloaderScheduler);

        Queue<KeyValuePair<string, BitmapImageWithDisposeHandle>> queueCache;
        int queueLimit;
        int currentQueueCount = 0;

        public ImageCache(int queueLimit = int.MaxValue)
        {
            this.queueLimit = queueLimit;

            this.queueCache = new Queue<KeyValuePair<string, BitmapImageWithDisposeHandle>>();

            #region old
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
            #endregion
        }

        public void Flush()
        {
            requestQueue.Flush();

            if (queueCache == null)
                return;

            //if (cache.Count == 0)
            //    return;

            //lock (syncHandle)
            {
                var enumerator = queueCache.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var o = enumerator.Current;
                    if (o.Value != null)
                        o.Value.Dispose();
                }

                queueCache.Clear();
                currentQueueCount = 0;
            }

            ////DebugEx.WriteLine("Flushing image cache");
            //lock (syncHandle)
            //{
            //    foreach (var image in cache.Values)
            //    {
            //        var capturedImage = image;
            //        GlobalDispatcher.Current.BeginInvoke(() =>
            //        {
            //            capturedImage.UriSource = null;
            //            //capturedImage.SetSource(null);
            //            capturedImage = null;
            //        });
            //    }
            //    cache.Clear();
            //}
            //cache = new Dictionary<string, BitmapImage>();
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect(); 
        }



        class BitmapImageWithDisposeHandle : IDisposable
        {
            public BitmapImage BitmapImage { get; set; }
            public IDisposable DisposeHandle { get; set; }

            public void Dispose()
            {
                GlobalDispatcher.Current.BeginInvoke(() =>
                {
                    if (BitmapImage != null)
                    {
                        BitmapImage.UriSource = null;
                        BitmapImage = null;
                    }
                });

                if (DisposeHandle != null)
                    DisposeHandle.Dispose();
            }
        }



        public IObservable<ImageSource> GetImageAsync(string url)
        {
            return Observable.Create<ImageSource>(observer =>
            {
                CompositeDisposable disposables = new CompositeDisposable();
                try
                {
                    Tuple<bool, BitmapImageWithDisposeHandle> attemptToRetrieve;

                    //lock (syncHandle)
                    {
                        attemptToRetrieve = queueCache.TryGet(url);
                    }

                    if (attemptToRetrieve.Item1 && attemptToRetrieve.Item2 != null)
                    {
                        observer.OnNext(attemptToRetrieve.Item2.BitmapImage);
                        observer.OnCompleted();
                    }

                    else
                    {
                        var newBitmap = new BitmapImage { CreateOptions = BitmapCreateOptions.IgnoreImageCache | BitmapCreateOptions.BackgroundCreation };

                        var newBitmapImageWithDisposeHandle = new BitmapImageWithDisposeHandle
                        {
                            BitmapImage = newBitmap,
                            DisposeHandle = disposables,
                        };

                        var newKvp = new KeyValuePair<string, BitmapImageWithDisposeHandle>(url, newBitmapImageWithDisposeHandle);

                        currentQueueCount++;

                        if (currentQueueCount > queueLimit)
                        {
                            var tailItem = queueCache.Dequeue();

                            currentQueueCount--;

                            //DebugEx.WriteLine("Removing {0}", tailItem.Key);

                            var bitmapImageWithDisposeHandle = tailItem.Value;
                            if (bitmapImageWithDisposeHandle != null)
                                bitmapImageWithDisposeHandle.Dispose();
                        }

                        //DebugEx.WriteLine("Adding {0}", url);
                        queueCache.Enqueue(newKvp);
      

                        newBitmap.UriSource = url.ToUri();

                        Observable.FromEventPattern<RoutedEventArgs>(newBitmap, "ImageOpened")
                            .SafelySubscribe(() => observer.OnNext(newBitmap), observer.OnError)
                            .DisposeWith(disposables);

                        Observable.FromEventPattern<ExceptionRoutedEventArgs>(newBitmap, "ImageFailed")
                            .SafelySubscribe(o => observer.OnError(o.EventArgs.ErrorException), observer.OnError)
                            .DisposeWith(disposables);

                        disposables.Add(Disposable.Create(() =>
                        {
                            if (newBitmap != null)
                                newBitmap.UriSource = null;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
                return disposables;
            });
        }

        //public void Dispose()
        //{
        //    this.Flush();
        //}
    }
}
