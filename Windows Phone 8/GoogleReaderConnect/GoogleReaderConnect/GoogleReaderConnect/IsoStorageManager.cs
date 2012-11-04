using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

 namespace GoogleReaderConnect
 {
    internal static class IsolatedStorageService
    {
        const string GOOGLE_READER_CREDENTIALS_KEY = "grc0";

        #region GENERIC HELPER METHODS

        static T Get<T>(string key, Func<Stream, T> deserializer)
        {
            T result = default(T);
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
            }
            watch.Stop();

            return result;
        }

        static void Save<T>(T obj, string key, Action<Stream, T> serializer)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = store.CreateFile(key))
                    {
                        serializer(stream, obj);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            watch.Stop();
        }

        #endregion

        internal static CredentialObject GetGoogleReaderCredentials()
        {
            var serializer = new DataContractSerializer(typeof(CredentialObject));
            return Get <CredentialObject>(GOOGLE_READER_CREDENTIALS_KEY,
                    stream => (CredentialObject)serializer.ReadObject(stream));
        }

        internal static void SaveCredentials(CredentialObject credentials)
        {
            var serializer = new DataContractSerializer(typeof(CredentialObject));
            Save(credentials, GOOGLE_READER_CREDENTIALS_KEY, (stream, obj) => serializer.WriteObject(stream, obj));
        }
    }
 }
