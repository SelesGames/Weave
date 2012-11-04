using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace weave
{
    public static class ObservableResource
    {
        private const string CacheFolder = "cache3";

        //public static async void CacheResource(MemoryStream toCache, string cacheKey)
        //{
        //    string targetFile = Path.Combine(CacheFolder, cacheKey);
        //    using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        if (!isoFile.DirectoryExists(CacheFolder))
        //            isoFile.CreateDirectory(CacheFolder);
        //        if (isoFile.FileExists(targetFile))
        //            isoFile.DeleteFile(targetFile);
        //        using (IsolatedStorageFileStream outputStream = isoFile.CreateFile(targetFile))
        //        {
        //            var buffer = toCache.GetBuffer();
        //            int chunkSize = 1024;

        //            for (int i = 0; i * chunkSize < buffer.Length; i++)
        //            {
        //                await Task.Factory.FromAsync(outputStream.BeginWrite, outputStream.EndWrite, buffer, i * chunkSize, chunkSize, null);
        //            }
        //        }
        //    }
        //}

        public static void CacheResource(MemoryStream toCache, string cacheKey)
        {
            string targetFile = Path.Combine(CacheFolder, cacheKey);
            using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoFile.DirectoryExists(CacheFolder))
                    isoFile.CreateDirectory(CacheFolder);
                if (isoFile.FileExists(targetFile))
                    isoFile.DeleteFile(targetFile);
                using (IsolatedStorageFileStream outputStream = isoFile.CreateFile(targetFile))
                {
                    toCache.WriteTo(outputStream);
                }
            }
        }

        public static IObservable<MemoryStream> FromCache(string cacheKey)
        {
            return Observable.Create<MemoryStream>(
                observer =>
                {
                    string targetFile = Path.Combine(CacheFolder, cacheKey);

                    using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!isoFile.FileExists(targetFile))
                        {
                            observer.OnNext(null);
                        }
                        else
                        {
                            using (IsolatedStorageFileStream inputStream =
                                isoFile.OpenFile(targetFile, FileMode.Open, FileAccess.Read))
                            {
                                observer.OnNext(inputStream.ToMemoryStream());
                            }
                        }
                    }
                    observer.OnCompleted();
                    return Disposable.Empty;
                });
        }

        //public static IObservable<MemoryStream> FromInternet(Uri onlineResource)
        //{
        //    return
        //        new AnonymousObservable<WebResponse>(
        //            observer =>
        //            {
        //                var httpWebRequest =
        //                    (HttpWebRequest)WebRequest.Create(onlineResource);
        //                httpWebRequest.BeginGetResponse(
        //                    iar =>
        //                    {
        //                        WebResponse response;
        //                        try
        //                        {
        //                            var requestState = (HttpWebRequest)iar.AsyncState;
        //                            response = requestState.EndGetResponse(iar);
        //                        }
        //                        catch (Exception exception)
        //                        {
        //                            observer.OnError(exception);
        //                            return;
        //                        }
        //                        observer.OnNext(response);
        //                        observer.OnCompleted();
        //                    }, httpWebRequest);
        //                return Disposable.Empty;
        //            })
        //            .Select(
        //                response => response.GetResponseStream().ToMemoryStream());
        //}
    }
}
