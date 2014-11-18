using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.IsoStorage
{
    internal abstract class IsoStorageClient<T>
    {
        public bool UseGzip { get; set; }




        #region Get/Read operation

        public async Task<T> GetAsync(string fileName, CancellationToken cancellationToken)
        {
            T result;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                result = await GetAsync(store, fileName, cancellationToken).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<T> GetAsync(IsolatedStorageFile store, string fileName, CancellationToken cancellationToken)
        {
            T result;

            if (store == null)
                throw new ArgumentNullException("store in IsoStorageClient.GetAsync");
            if (!store.FileExists(fileName))
                throw new FileNotFoundException(string.Format("File {0} not found during IsoStorageClient.GetAsync", fileName)); ;


            using (IsolatedStorageFileStream stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
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
                        using (var gzip = new GZipInputStream(ms))
                        {
                            result = ReadObject(gzip);
                            gzip.Close();
                        }
                    }
                    else
                    {
                        result = ReadObject(ms);
                    }
                    ms.Close();
                }
                stream.Close();
            }
            return result;
        }

        protected abstract T ReadObject(Stream stream);

        #endregion




        #region Save/Write operation

        public async Task SaveAsync(string fileName, T obj, CancellationToken cancellationToken)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                await SaveAsync(store, fileName, obj, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(IsolatedStorageFile store, string fileName, T obj, CancellationToken cancellationToken)
        {
            using (var ms = new MemoryStream())
            {
                WriteObject(obj, ms);

                ms.Position = 0;

                using (IsolatedStorageFileStream stream = store.CreateFile(fileName))
                //using (IsolatedStorageFileStream stream = store.OpenFile(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    int bufferSize = 1024 * 1024;
                    await ms.CopyToAsync(stream, bufferSize).ConfigureAwait(false);
                    //await stream.WriteAsync(buffer, 0, length);
                    stream.Close();
                }

                ms.Close();
            }
        }

        protected abstract void WriteObject(T obj, Stream stream);

        #endregion
    }
}
