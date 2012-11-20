using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.IsoStorage
{
    public class SafeIsoStorageFileWrapper<T>
    {
        string localFileName;
        string internalFileName;

        DelegateIsoStorageClient<T> isoStorageClient;
        DelegateIsoStorageClient<string> keyClient;

        public SafeIsoStorageFileWrapper(string localFileName, Action<Stream, T> serializer, Func<Stream, T> deserializer)
        {
            if (localFileName == null)  throw new ArgumentNullException("localFileName");
            if (serializer == null)     throw new ArgumentNullException("serializer");
            if (deserializer == null)   throw new ArgumentNullException("deserializer");


            this.localFileName = localFileName;


            this.isoStorageClient = new DelegateIsoStorageClient<T>(deserializer, (obj, stream) => serializer(stream, obj));
            this.keyClient = new DelegateIsoStorageClient<string>(
                stream =>
                {
                    var reader = new StreamReader(stream);
                    var result = reader.ReadLine();
                    return result;
                },
                (key, stream) =>
                {
                    //using (var writer = new StreamWriter(stream))
                    //{
                        var writer = new StreamWriter(stream);
                        writer.Write(key);
                        writer.Flush();
                    //}
                });

            var t = keyClient.GetAsync(localFileName, CancellationToken.None);
            try
            {
                t.Wait();
                this.internalFileName = t.Result;
            }
            catch { }
        }

        public async Task<T> Get()
        {
            if (string.IsNullOrEmpty(this.internalFileName))
            {
                return Activator.CreateInstance<T>();
            }

            var recoveredInstance = await this.isoStorageClient.GetAsync(this.internalFileName, CancellationToken.None).ConfigureAwait(false);

            if (recoveredInstance != null)
                return recoveredInstance;
            else
                return Activator.CreateInstance<T>();
        }


        public async Task<bool> Save(T update)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string newFileName = this.localFileName + Guid.NewGuid().ToString();
                    await this.isoStorageClient.SaveAsync(store, newFileName, update, CancellationToken.None).ConfigureAwait(false);

                    var oldFileName = this.internalFileName;
                    this.internalFileName = newFileName;

                    await this.keyClient.SaveAsync(store, localFileName, internalFileName, CancellationToken.None).ConfigureAwait(false);

                    if (store.FileExists(oldFileName))
                        store.DeleteFile(oldFileName);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
    }

}
