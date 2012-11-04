using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using ProtoBuf;

namespace weave
{
    public static class IsolatedStorageService
    {
        #region string keys

        //const string FEEDS_KEY = "dk9n";
        //const string CATKEY = "allcats";
        //const string TOP8_PANORAMA_NEWS = "t8pn";
        const string TOMBSTONE_STATE = "tmbstt";
        //const string INSTAPAPER_CREDENTIALS = "instppercred";
        const string STARRED_NEWS = "strredNews";
        //const string PERMA_STATE = "permstt";

        #endregion



        #region GENERIC HELPER METHODS

        public static T Get<T>(string key)
        {
            return Get<T>(key, stream => Serializer.Deserialize<T>(stream));
        }

        public static T Get<T>(string key, Func<Stream, T> deserializer)
        {
            T result = default(T);

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

            return result;
        }


        public static void Save<T>(T obj, string key)
        {
            Save(obj, key, (stream, obj2) => Serializer.Serialize(stream, obj2));
        }

        public static void Save<T>(T obj, string key, Action<Stream, T> serializer)
        {
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
                DebugEx.WriteLine("Problem writing to IsoStorageFile:\r\n{0}", ex);
            }
        }

        #endregion




        #region Images

        //internal static ImageSource GetImage(string imageUri)
        //{
        //    string file = "Images\\" + imageUri.GetHashCode() + ".png";

        //    DebugEx.WriteLine("Starting to read {0} from IsoStorageStream", file);
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
            
        //    try
        //    {
        //        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
        //        {
        //            if (store == null)
        //                return null;

        //            if (!store.FileExists(file))
        //                return null;

        //            using (IsolatedStorageFileStream isfs = store.OpenFile(file, FileMode.Open))
        //            {
        //                if (isfs.Length > 0)
        //                {
        //                    BitmapImage image = new BitmapImage();
        //                    image.SetSource(isfs);
        //                    isfs.Close();

        //                    watch.Stop();
        //                    DebugEx.WriteLine("Finished reading {1} - took {0} seconds", watch.Elapsed.TotalSeconds, file);

        //                    return image;
        //                }
        //                return null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DebugEx.WriteLine("Problem with reading from IsoStorageFile:\r\n{0}", ex);
        //        return null;
        //    }
        //}

        //internal static void SaveImage(Stream s, string imageUri)
        //{
        //    string file = "Images\\" + imageUri.GetHashCode() + ".png";

        //    DebugEx.WriteLine("Starting to write {0} to IsoStorageStream", file);
        //    var watch = System.Diagnostics.Stopwatch.StartNew();

        //    try
        //    {
        //        using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
        //        {
        //            if (!store.DirectoryExists("Images"))
        //                store.CreateDirectory("Images");

        //            using (IsolatedStorageFileStream isfs = store.CreateFile(file))
        //            {
        //                int count = 0;
        //                byte[] buffer = new byte[1024];
        //                s.Seek(0, SeekOrigin.Begin);
        //                while (0 < (count = s.Read(buffer, 0, buffer.Length)))
        //                {
        //                    isfs.Write(buffer, 0, count);
        //                }
        //                isfs.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DebugEx.WriteLine("Problem writing to IsoStorageFile:\r\n{0}", ex);
        //    }
        //    watch.Stop();
        //    DebugEx.WriteLine("Finished writing {1} - took {0} seconds", watch.Elapsed.TotalSeconds, file);
        //}

        #endregion




        #region Starred News Items

        //internal static List<StarredNewsItem> GetStarredNewsItems()
        //{
        //    return Get<List<StarredNewsItem>>(STARRED_NEWS);
        //}

        //internal static void SaveStarredNewsItems(List<StarredNewsItem> starredNewsItems)
        //{
        //    Save(starredNewsItems, STARRED_NEWS);
        //}

        #endregion
    }
}
