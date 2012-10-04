
namespace System.IO.IsolatedStorage
{
    public class SafeIsoStorageFileWrapper<T>
    {
        string localFileName;
        Action<Stream, T> serializer;
        Func<Stream, T> deserializer;
        string internalFileName;
        IsolatedStorageService isoStorageService;

        public SafeIsoStorageFileWrapper(string localFileName, Action<Stream, T> serializer, Func<Stream, T> deserializer)
        {
            if (localFileName == null)
                throw new ArgumentNullException("localFileName");

            if (serializer == null)
                throw new ArgumentNullException("serializer");

            if (deserializer == null)
                throw new ArgumentNullException("deserializer");


            this.localFileName = localFileName;
            this.serializer = serializer;
            this.deserializer = deserializer;

            //var serializer = new DataContractSerializer(typeof(string));
            this.isoStorageService = new IsolatedStorageService();// ServiceResolver.Get<IsolatedStorageService>();
            this.internalFileName = this.isoStorageService.Get<string>(this.localFileName, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadLine();
                    reader.Close();
                    return result;
                }
            });
        }

        public T Get()
        {
            if (string.IsNullOrEmpty(this.internalFileName))
            {
                return Activator.CreateInstance<T>();
            }

            var recoveredInstance = this.isoStorageService.Get(this.internalFileName, this.deserializer);

            if (recoveredInstance != null)
                return recoveredInstance;
            else
                return Activator.CreateInstance<T>();
        }


        public bool Save(T update)
        {
            //DebugEx.WriteLine("Starting to write TYPE: {0} to IsoStorageStream", typeof(T).Name);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string fileName = this.localFileName + Guid.NewGuid().ToString();
                    using (IsolatedStorageFileStream stream = store.CreateFile(fileName))
                    {
                        this.serializer(stream, update);
                        stream.Flush();
                        stream.Close();
                    }
                    var oldFileName = this.internalFileName;
                    this.internalFileName = fileName;

                    using (IsolatedStorageFileStream stream = store.CreateFile(localFileName))
                    using (var writer = new StreamWriter(stream))
                    {
                        //var serializer = new DataContractSerializer(typeof(string));
                        //serializer.WriteObject(stream, this.internalFileName);
                        //new StreamWriter(stream).Write(this.internalFileName);
                        writer.Write(this.internalFileName);
                        writer.Flush();
                        stream.Flush();
                        writer.Close();
                        stream.Close();
                    }

                    if (store.FileExists(oldFileName))
                        store.DeleteFile(oldFileName);

                    return true;
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("Problem writing TYPE: {1} to IsoStorageFile:\r\n{0}", ex, typeof(T).Name);
                return false;
            }
            finally
            {
                watch.Stop();
                DebugEx.WriteLine("Finished writing TYPE: {1} to IsoStorageStream - took {0} ms", watch.ElapsedMilliseconds, typeof(T).Name);
            }
        }
    }

}
