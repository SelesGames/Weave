//using System;
//using System.Collections.Generic;
//using System.IO.IsolatedStorage;
//using System.Linq;
//using ProtoBuf;

//namespace weave
//{
//    [ProtoContract]
//    public class NewsLookup
//    {
//        [ProtoMember(1)] public Guid FeedId { get; set; }
//        [ProtoMember(2)] public List<NewsItem> News { get; set; }
//    }

//    [ProtoContract]
//    public class NewsLookupsWrapper // : List<NewsLookup>
//    {
//        [ProtoMember(1)] public List<NewsLookup> NewsLookups { get; set; }

//        public NewsLookupsWrapper()
//        {
//            NewsLookups = new List<NewsLookup>();
//        }
//    }
    


//    public class NewsRepository
//    {
//        readonly string LOCAL_KEY = "nwzRp";

//        NewsLookupsWrapper newsLookups;// = new NewsLookups();

//        SafeIsoStorageFileWrapper<NewsLookupsWrapper> safeWrapper;

//        public NewsRepository()
//        {
//            safeWrapper = new SafeIsoStorageFileWrapper<NewsLookupsWrapper>(
//                LOCAL_KEY,
//                ProtoBuf.Serializer.Serialize,
//                ProtoBuf.Serializer.Deserialize<NewsLookupsWrapper>);

//            this.newsLookups = safeWrapper.Get();
//        }

//        internal List<NewsItem> GetNews(FeedSource feed)
//        {
//            Guid key = feed.Id;
//            var existingEntry = this.newsLookups.NewsLookups.Where(o => o.FeedId == key).FirstOrDefault();

//            if (existingEntry != null && existingEntry.News != null)
//            {
//                foreach (var newsItem in existingEntry.News)
//                    newsItem.FeedSource = feed;

//                return existingEntry.News;
//            }
//            else
//                return null;
//        }

//        internal void SaveNews(FeedSource feed)
//        {
//            Guid key = feed.Id;
//            var existingEntry = this.newsLookups.NewsLookups.Where(o => o.FeedId == key).FirstOrDefault();

//            if (existingEntry == null)
//            {
//                existingEntry = new NewsLookup { FeedId = key };
//                this.newsLookups.NewsLookups.Add(existingEntry);
//            }

//            existingEntry.News = feed.News;
//        }

//        internal void WriteToIsoStorage()
//        {
//            this.safeWrapper.Save(this.newsLookups);
//        }
//    }
//}
