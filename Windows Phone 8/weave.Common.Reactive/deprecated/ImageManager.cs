using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Reactive.Subjects;
using System.Reactive;


namespace weave
{
    public class ImageManager
    {
        public static ImageManager Instance = new ImageManager();

        Dictionary<string, Task> cache = new Dictionary<string,Task>();

        public async Task<ImageSource> CreateThumbnailImage(Uri uri, int width)
        {
            var fileName = CreateFileNameFromUriString(uri.OriginalString);
            if (cache.ContainsKey(fileName))
            {
                try
                {
                    var recoveredImage = await GetImage(fileName, width, width);
                    if (recoveredImage != null)
                        return recoveredImage;
                }
                catch { }
            }

            var request = System.Net.Browser.WebRequestCreator.ClientHttp.Create(uri);

            var stream = await request.GetWebResponseFullyAsync().Select(o => o.GetResponseStream()).ToTask();

            BitmapImage bi = new BitmapImage { CreateOptions = BitmapCreateOptions.BackgroundCreation };

            var holder = new AsyncSubject<Unit>();
            bi.ImageOpenedOrFailed().Subscribe(o => holder.OnNext(o), holder.OnError, holder.OnCompleted);
            bi.SetSource(stream);

            stream.Close();
            stream.Dispose();


            //BitmapImage bi = new BitmapImage(uri) { CreateOptions = BitmapCreateOptions.BackgroundCreation };// | BitmapCreateOptions.IgnoreImageCache };
            ImageBrush ib = new ImageBrush { Stretch = Stretch.UniformToFill };
            ib.ImageSource = bi;
            Rectangle rect = new Rectangle { StrokeThickness = 0, Width = width, Height = width, };
            rect.Fill = ib;

                        await holder.ToTask();


            //await bi.ImageOpenedOrFailed().ToTask();


            #region create image copies

            //var sw = System.Diagnostics.Stopwatch.StartNew();
            WriteableBitmap wb1 = new WriteableBitmap(width, width);
            wb1.Render(rect, null);
            wb1.Invalidate();
            //sw.Stop();
            //DebugEx.WriteLine("took {0} ms to copy image1", sw.ElapsedMilliseconds);


            //sw = System.Diagnostics.Stopwatch.StartNew();
            var wb2 = wb1.Clone();// new WriteableBitmap(wb1);
            //WriteableBitmap wb2 = new WriteableBitmap(width, width);
            //for (int i = 0; i < wb2.Pixels.Length; i++)
            //    wb2.Pixels[i] = wb1.Pixels[i];
            //wb2.Invalidate();
            //sw.Stop();
            //DebugEx.WriteLine("took {0} ms to copy image2", sw.ElapsedMilliseconds);
            #endregion


            SaveImage(uri.OriginalString, wb2);


            #region fully dispose of all temp images
            wb1 = null;
            rect = null;
            ib.ImageSource = null;
            ib = null;
            bi.UriSource = null;
            bi = null;
            #endregion


            return wb2;
        }

        async Task<WriteableBitmap> GetImage(string fileName, int imageWidth, int imageHeight)
        {
            var val = cache[fileName];

            if (val == null)
                return null;

            await val;

            var temp = await TaskEx.Run(() => GetImageByteData(fileName));
            var data = await temp;

            var existingImage = new WriteableBitmap(imageWidth, imageHeight);
            existingImage.FromByteArray(data);
            return existingImage;
        }

        async Task<byte[]> GetImageByteData(string key)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store == null) throw new Exception("no iso storage store");
                if (!store.FileExists(key)) throw new Exception("file does not exist");

                using (IsolatedStorageFileStream stream = store.OpenFile(key, FileMode.Open))
                {
                    if (stream.Length <= 0) throw new Exception("problem reading image from iso store");

                    byte[] buffer = new byte[stream.Length];
                    var result = await stream.ReadAsync(buffer, 0, (int)stream.Length);
                    stream.Close();
                    return buffer;
                }
            }
        }

        void SaveImage(string uri, WriteableBitmap bitmap)
        {
            var fileName = CreateFileNameFromUriString(uri);

            if (cache.ContainsKey(fileName))
                return;

            var data = bitmap.ToByteArray();

            try
            {
                Task t = TaskEx.Run(async () =>
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    using (IsolatedStorageFileStream stream = store.CreateFile(fileName))
                    {
                        await stream.WriteAsync(data, 0, data.Length);
                        stream.Close();
                    }
                });

                cache.Add(fileName, t);
            }
            catch
            {
                if (cache.ContainsKey(fileName))
                    cache.Remove(fileName);
            }
        }

        string CreateFileNameFromUriString(string uri)
        {
            return uri.GetHashCode().ToString();
        }



        public IObservable<ImageSource> Web(Uri uri, int width)
        {
            return Observable.Create<ImageSource>(observer =>
            {
                var disposables = new CompositeDisposable();

                try
                {
                    var request = System.Net.Browser.WebRequestCreator.ClientHttp.Create(uri);
                    bool didRequestComplete = false;
                    disposables.Add(Disposable.Create(() =>
                    {
                        try
                        {
                            if (!didRequestComplete)
                                request.Abort();
                        }
                        catch { }
                    }));

                    request.GetWebResponseFullyAsync().ObserveOnDispatcher().SafelySubscribe(response =>
                    {
                        didRequestComplete = true;
                        BitmapImage bi = new BitmapImage { CreateOptions = BitmapCreateOptions.BackgroundCreation };
                        AsyncSubject<Unit> imagepopped = new AsyncSubject<Unit>();
                        bi.ImageOpenedOrFailed().Subscribe(o => imagepopped.OnNext(o), imagepopped.OnError, imagepopped.OnCompleted).DisposeWith(disposables);

                        var stream = response.GetResponseStream();
                        //{
                            bi.SetSource(stream);

                            ImageBrush ib = new ImageBrush { Stretch = Stretch.UniformToFill };
                            ib.ImageSource = bi;
                            //disposables.Add(Disposable.Create(() => ib.ImageSource = null));
                            Rectangle rect = new Rectangle { StrokeThickness = 0, Width = width, Height = width, };
                            rect.Fill = ib;

                            //stream.Close();
                        //}



                        imagepopped.SafelySubscribe(() =>
                        {
                            #region create image copies

                            //var sw = System.Diagnostics.Stopwatch.StartNew();
                            WriteableBitmap wb1 = new WriteableBitmap(width, width);
                            wb1.Render(rect, null);
                            wb1.Invalidate();
                            //sw.Stop();
                            //DebugEx.WriteLine("took {0} ms to copy image1", sw.ElapsedMilliseconds);


                            //sw = System.Diagnostics.Stopwatch.StartNew();
                            var wb2 = wb1.Clone();// new WriteableBitmap(wb1);
                            //WriteableBitmap wb2 = new WriteableBitmap(width, width);
                            //for (int i = 0; i < wb2.Pixels.Length; i++)
                            //    wb2.Pixels[i] = wb1.Pixels[i];
                            //wb2.Invalidate();
                            //sw.Stop();
                            //DebugEx.WriteLine("took {0} ms to copy image2", sw.ElapsedMilliseconds);
                            #endregion


                            SaveImage(uri.OriginalString, wb2);

                            ib.ImageSource = null;
                            bi.UriSource = null;

                            observer.OnNext(wb2);
                            observer.OnCompleted();
                        }, observer.OnError, observer.OnCompleted).DisposeWith(disposables);
                    }, observer.OnError, observer.OnCompleted).DisposeWith(disposables);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
                return disposables;
            });
        }

        public IObservable<ImageSource> CreateThumbnailImageAsync(Uri uri, int width)
        {
            return Observable.Create<ImageSource>(observer =>
            {
                var disposables = new CompositeDisposable();

                try
                {
                    var fileName = CreateFileNameFromUriString(uri.OriginalString);
                    if (cache.ContainsKey(fileName))
                    {
                        GetImage(fileName, width, width).ToObservable().SafelySubscribe(
                            recoveredImage =>
                            {
                                if (recoveredImage != null)
                                {
                                    observer.OnNext(recoveredImage);
                                    observer.OnCompleted();
                                }
                                else
                                    Web(uri, width).Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted).DisposeWith(disposables);
                            }, 
                            ex => Web(uri, width).Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted).DisposeWith(disposables), 
                            observer.OnCompleted)
                        .DisposeWith(disposables);
                    }
                    else
                        Web(uri, width).Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted).DisposeWith(disposables);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
                return disposables;
            });
        }
    }
}
