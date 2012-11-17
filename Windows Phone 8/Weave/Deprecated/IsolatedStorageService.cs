
namespace System.IO.IsolatedStorage
{
    public class IsolatedStorageService
    {
        #region GENERIC HELPER METHODS

        public T Get<T>(string key, Func<Stream, T> deserializer)
        {
            T result = default(T);
            //DebugEx.WriteLine("Starting to read {0} from IsoStorageStream", typeof(T).Name);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store == null)
                        return default(T);

                    if (!store.FileExists(key))
                        return default(T);

                    using (IsolatedStorageFileStream stream = store.OpenFile(key, FileMode.Open))
                    {
                        if (stream.Length > 0)
                        {
                            result = deserializer(stream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("Problem with reading from IsoStorageFile:\r\n{0}", ex);
            }
            watch.Stop();
            DebugEx.WriteLine("Finished reading {1} - took {0} seconds", watch.Elapsed.TotalSeconds, typeof(T).Name);

            return result;
        }

        //public void Save<T>(T obj, string key, Action<Stream, T> serializer)
        //{
        //    //DebugEx.WriteLine("Starting to write {0} to IsoStorageStream", typeof(T).Name);
        //    var watch = System.Diagnostics.Stopwatch.StartNew();

        //    try
        //    {
        //        using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
        //        {
        //            if (store.FileExists(key))
        //                store.DeleteFile(key);

        //            using (IsolatedStorageFileStream stream = store.CreateFile(key))
        //            {
        //                serializer(stream, obj);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DebugEx.WriteLine("Problem writing to IsoStorageFile:\r\n{0}", ex);
        //    }
        //    watch.Stop();
        //    DebugEx.WriteLine("Finished writing {1} - took {0} seconds", watch.Elapsed.TotalSeconds, typeof(T).Name);
        //}

        #endregion
    }
}
