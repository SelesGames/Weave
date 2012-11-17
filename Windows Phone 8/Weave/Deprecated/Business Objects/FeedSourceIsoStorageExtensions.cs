//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.IsolatedStorage;
//using System.Linq;
//using ProtoBuf;

//namespace weave
//{
//    public static class FeedSourceIsoStorageExtensions
//    {
//        public static Tuple<bool, int> TryParseAsInt(this string s)
//        {
//            int x;
//            if (int.TryParse(s, out x))
//                return Tuple.Create(true, x);
//            else
//                return Tuple.Create(false, x);
//        }

//        public static List<NewsItem> GetAllOldNewsItems(this FeedSource feedSource)
//        {
//            var watch = System.Diagnostics.Stopwatch.StartNew();

//            try
//            {
//                var directory = feedSource.FeedUri.GetHashCode().ToString();

//                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
//                {
//                    if (store == null)
//                        return null;// default(T);

//                    if (!store.DirectoryExists(directory))
//                        return null;

//                    var fileNumbers = store.GetFileNames(directory + "/")
//                        //.Select(o => o.Substring(o.IndexOf('/')))
//                        .Select(o => o.TryParseAsInt())
//                        .Where(o => o.Item1)
//                        .Select(o => o.Item2)
//                        .OrderByDescending(o => o);

//                    var aggregator = new List<NewsItem>();

//                    foreach (var fileNumber in fileNumbers)
//                    {
//                        var fullPath = string.Format("{0}/{1}", directory, fileNumber.ToString());

//                        using (IsolatedStorageFileStream stream = store.OpenFile(fullPath, FileMode.Open, FileAccess.Read))
//                        {
//                            if (stream.Length > 0)
//                            {
//                                var result = Serializer.Deserialize<List<NewsItem>>(stream);
//                                aggregator.AddRange(result);
//                            }
//                        }
//                    }

//                    return aggregator;
//                }
//            }
//            catch (Exception ex)
//            {
//                DebugEx.WriteLine("Problem with reading from IsoStorageFile:\r\n{0}", ex);
//            }
//            watch.Stop();
//            DebugEx.WriteLine("Finished reading {1} - took {0} seconds", watch.Elapsed.TotalSeconds, feedSource.FeedName);

//            return null;
//        }

//        public static void SaveNewsItems(this FeedSource feedSource, List<NewsItem> news, bool isOldNewsLoadedCompleted)
//        {
//            var watch = System.Diagnostics.Stopwatch.StartNew();

//            try
//            {
//                if (news == null || news.Count == 0)
//                    return;

//                var directory = feedSource.FeedUri.GetHashCode().ToString();

//                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
//                {
//                    if (store == null)
//                        return;

//                    if (!store.DirectoryExists(directory))
//                        store.CreateDirectory(directory);

//                    if (isOldNewsLoadedCompleted)
//                    {
//                        var filesToDelete = store.GetFileNames(directory + "/");
//                        foreach (var file in filesToDelete)
//                            //store.DeleteFile(file);
//                            store.DeleteFile(string.Format("{0}/{1}", directory, file));

//                        using (IsolatedStorageFileStream stream = store.CreateFile(directory + "/1"))
//                        {
//                            Serializer.Serialize<List<NewsItem>>(stream, news);
//                        }
//                    }

//                    else
//                    {
//                        var fileNumbers = store.GetFileNames(directory + "/")
//                            //.Select(o => o.Substring(o.IndexOf('/')))
//                            .Select(o => o.TryParseAsInt())
//                            .Where(o => o.Item1)
//                            .Select(o => o.Item2)
//                            .OrderByDescending(o => o);

//                        var index = fileNumbers.FirstOrDefault();
//                        index++;

//                        using (IsolatedStorageFileStream stream = store.CreateFile(directory + "/" + index.ToString()))
//                        {
//                            Serializer.Serialize<List<NewsItem>>(stream, news);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                DebugEx.WriteLine("Problem with writing to IsoStorageFile:\r\n{0}", ex);
//            }
//            watch.Stop();
//            DebugEx.WriteLine("Finished writing {1} - took {0} seconds", watch.Elapsed.TotalSeconds, feedSource.FeedName);
//        }
//    }
//}
