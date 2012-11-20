using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SelesGames.IsoStorage
{
    public abstract class IsoStorageClient<T>
    {
        public bool UseGzip { get; set; }




        #region Get/Read operation

        public Task<T> GetAsync(string fileName, CancellationToken cancellationToken)
        {
            var folder = ApplicationData.Current.LocalFolder;
            return GetAsync(folder, fileName, cancellationToken);
        }

        public async Task<T> GetAsync(StorageFolder folder, string fileName, CancellationToken cancellationToken)
        {
            if (folder == null) throw new ArgumentNullException("folder: SelesGames.IsoStorage.GetAsync");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("please specify a fileName: SelesGames.IsoStorage.GetAsync");

            var file = await folder.GetFileAsync(fileName);
            if (file == null)
                throw new ArgumentNullException("store in IsoStorageClient.GetAsync");
            //if (!store.FileExists(fileName))
            //    throw new FileNotFoundException(string.Format("File {0} not found during IsoStorageClient.GetAsync", fileName)); ;

            T result;

            using (var fileStream = await file.OpenSequentialReadAsync())
            using (var stream = fileStream.AsStreamForRead())
            {
                var length = (int)stream.Length;
                if (length <= 0)
                    throw new FileNotFoundException(string.Format("File {0} is missing", fileName));

                var buffer = new byte[length];
                var numberOfBytesRead = await stream.ReadAsync(buffer, 0, length).ConfigureAwait(false);

                if (numberOfBytesRead != length)
                    throw new Exception("Unexpected number of bytes read from IsoStorageClient.GetAsync");

                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException("request to read from isoStorage was cancelled");

                using (var ms = new MemoryStream(buffer))
                {
                    ms.Position = 0;
                    if (UseGzip)
                    {
                        using (var gzip = new GZipStream(ms, CompressionLevel.Fastest, false))
                        {
                            result = ReadObject(gzip);
                        }
                    }
                    else
                    {
                        result = ReadObject(ms);
                    }
                }
            }
            return result;
        }

        protected abstract T ReadObject(Stream stream);

        #endregion




        #region Save/Write operation

        public async Task SaveAsync(string fileName, T obj, CancellationToken cancellationToken)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync())
            using (var stream = transaction.Stream.AsStreamForWrite())
            {
                WriteObject(obj, stream);
                await transaction.CommitAsync();
            }
        }

        //public async Task SaveAsync(IsolatedStorageFile store, string fileName, T obj, CancellationToken cancellationToken)
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        WriteObject(obj, ms);

        //        ms.Position = 0;

        //        using (IsolatedStorageFileStream stream = store.CreateFile(fileName))
        //        //using (IsolatedStorageFileStream stream = store.OpenFile(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
        //        {
        //            int bufferSize = 1024 * 1024;
        //            await ms.CopyToAsync(stream, bufferSize).ConfigureAwait(false);
        //            //await stream.WriteAsync(buffer, 0, length);
        //            stream.Close();
        //        }

        //        ms.Close();
        //    }
        //}

        protected abstract void WriteObject(T obj, Stream stream);

        #endregion
    }
}
