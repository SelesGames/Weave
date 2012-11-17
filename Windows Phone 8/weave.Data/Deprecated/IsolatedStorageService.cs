//using System.Collections.Generic;
//using System.Threading.Tasks;
//using ProtoBuf;
//using SelesGames.WP.IsoStorage;

//namespace weave.Data
//{
//    internal static class IsolatedStorageServiceExtensions
//    {
//        #region string keys

//        const string FEEDS_KEY = "dk9n";
//        const string CATKEY = "allcats";
//        const string NEWS_KEY = "news";

//        #endregion




//        #region Feeds

//        internal async static Task<List<FeedSource>> GetFeedsPost30()
//        {
//            var safe = new SafeIsoStorageFileWrapper<List<FeedSource>>(
//                FEEDS_KEY, Serializer.Serialize, Serializer.Deserialize<List<FeedSource>>);

//            var result = await safe.Get();

//            return result;
//        }

//        internal async static Task SaveFeedsToIsoStoragePost30(List<FeedSource> feeds)
//        {
//            var safe = new SafeIsoStorageFileWrapper<List<FeedSource>>(
//                FEEDS_KEY, Serializer.Serialize, Serializer.Deserialize<List<FeedSource>>);

//            await safe.Save(feeds);
//        }

//        #endregion




//        #region News

//        internal async static Task<List<NewsItem>> GetNews()
//        {
//            var safe = new SafeIsoStorageFileWrapper<List<NewsItem>>(
//                NEWS_KEY, Serializer.Serialize, Serializer.Deserialize<List<NewsItem>>);

//            var result = await safe.Get();

//            return result;
//        }

//        internal async static Task SaveNews(List<NewsItem> news)
//        {
//            var safe = new SafeIsoStorageFileWrapper<List<NewsItem>>(
//                NEWS_KEY, Serializer.Serialize, Serializer.Deserialize<List<NewsItem>>);

//            await safe.Save(news);
//        }

//        #endregion




//        #region Categories

//        internal async static Task<List<Category>> GetCategoriesPost30()
//        {
//            var safe = new SafeIsoStorageFileWrapper<List<Category>>(
//                CATKEY, Serializer.Serialize, Serializer.Deserialize<List<Category>>);

//            var result = await safe.Get();

//            return result;
//        }

//        internal async static Task SaveCategoriesToIsoStoragePost30(List<Category> categories)
//        {
//            var safe = new SafeIsoStorageFileWrapper<List<Category>>(
//                CATKEY, Serializer.Serialize, Serializer.Deserialize<List<Category>>);

//            await safe.Save(categories);
//        }

//        #endregion
//    }
//}
