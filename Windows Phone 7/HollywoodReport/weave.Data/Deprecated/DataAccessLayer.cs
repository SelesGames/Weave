using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using SelesGames.WP.IsoStorage;
using ProtoBuf;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace weave.Data
{
    public class DataAccessLayer
    {
        const string FEEDS_KEY = "dk9n";
        const string CATKEY = "allcats";
        const string NEWS_KEY = "news";

        bool hasLoadedData = false;
        string assemblyName;
        bool canAddCategories;
        bool canSelectInitialCategories;
        bool isFirstWeave30Launch;

        const string FILE = "Feeds.xml";
        List<Category> categories;
        List<FeedSource> feeds;

        object syncObject = new object();



        public DataAccessLayer(string assemblyName, bool canAddCategories, bool canSelectInitialCategories, bool isFirstWeave30Launch)
        {
            this.assemblyName = assemblyName;
            this.canAddCategories = canAddCategories;
            this.canSelectInitialCategories = canSelectInitialCategories;
            this.isFirstWeave30Launch = isFirstWeave30Launch;
        }




        #region Get and Save to/from isostorage

        static IsoStorageClient<T> CreateIsoClient<T>()
        {
            return new DelegateIsoStorageClient<T>(
                stream => Serializer.Deserialize<T>(stream),
                (o, stream) => Serializer.Serialize<T>(stream, o));
        }

        static async Task<T> Get<T>(string key)
        {
            try
            {
                return await CreateIsoClient<T>().GetAsync(key, CancellationToken.None);
            }
            catch
            {
                return Activator.CreateInstance<T>();
            }
        }

        static async Task Save<T>(string key, T obj)
        {
            try
            {
                await CreateIsoClient<T>().SaveAsync(key, obj, CancellationToken.None);
            }
            catch { }
        }

        #endregion




        async Task DoLoadCheck()
        {
            if (!hasLoadedData)
            {
                //lock (syncObject)
                //{
                var sw = System.Diagnostics.Stopwatch.StartNew();

                List<NewsItem> news = null;
                Func<Task> t = async () => 
                {
                    var sw2 = System.Diagnostics.Stopwatch.StartNew();
                    news = await Get<List<NewsItem>>(NEWS_KEY);
                    sw2.Stop();
                    DebugEx.WriteLine("###################  Took {0} ms to read news from disk", sw2.ElapsedMilliseconds);
                };

                await TaskEx.WhenAll(LoadCategories(), LoadFeeds(), t.Invoke());

                if (news != null)
                {
                    foreach (var feed in feeds)
                    {
                        var oldNewsCount = feed.NewsCount;

                        var feedsNews = news.Where(o => o.FeedId == feed.Id);
                        foreach (var newsItem in feedsNews)
                        {
                            newsItem.FeedSource = feed;
                            newsItem.HP--;
                            if (newsItem.HP <= 0)
                                oldNewsCount--;
                        }
                        feed.News = feedsNews.Where(o => o.HP > 0).ToList();

                        feed.NewsCount = oldNewsCount;
                    }
                }

                hasLoadedData = true;

                sw.Stop();
                DebugEx.WriteLine("took {0} ms to load data", sw.ElapsedMilliseconds);
            }
            //}
        }




        #region Public access methods

        public List<Category> GetAllCategories()
        {
            DoLoadCheck().Wait();

            List<Category> categoriesCopy;
            lock (syncObject)
                categoriesCopy = categories.ToList();

            return categoriesCopy;
        }

        public List<FeedSource> GetAllFeeds()
        {
            DoLoadCheck().Wait();

            List<FeedSource> feedsCopy;
            lock (syncObject)
                feedsCopy = feeds.ToList();
            return feedsCopy;
        }

        public IEnumerable<Category> GetEnabledCategories()
        {
            return GetAllCategories().Where(o => o.IsEnabled);
        }

        public Category GetCategory(string categoryName)
        {
            return GetAllCategories().SingleOrDefault(category => category.Name == categoryName);
        }

        public IEnumerable<FeedSource> GetEnabledFeeds()
        {
            return GetAllFeeds()
                .Where(feed => feed.IsEnabled)
                .Where(feed => CheckIfCategoryIsEnabledOrIfNoCategory(feed));
        }

        public IEnumerable<FeedSource> GetEnabledFeedsByCategory(string categoryName)
        {
            return GetEnabledFeeds()
                .Where(o => o.Category == categoryName);
        }

        public IEnumerable<NewsItem> GetAllNews()
        {
            return GetEnabledFeeds()
                .Where(o => o.News != null)
                .SelectMany(o => o.News)
                .OrderByDescending(o => o.PublishDateTime);
        }

        public void RefreshAllFeeds()
        {
            GetEnabledFeeds().ToList().ForEach(feed => feed.CheckForAndGetNewFeedData());
        }




        #region Add Feed or Category

        public bool AddCustomFeed(FeedSource newFeed)
        {
            DoLoadCheck().Wait();

            lock (syncObject)
            {
                if (feeds.Where(feed =>
                    feed.Category == newFeed.Category &&
                    feed.FeedUri == newFeed.FeedUri).Count() > 0)
                    return false;

                feeds.Add(newFeed);
                ReportingService.ReportCustomFeedAdded(newFeed.FeedUri);
            }
            newFeed.IsUserAdded = true;
            newFeed.Id = Guid.NewGuid();
            newFeed.CheckForAndGetNewFeedData();
            SaveFeeds();
            return true;
        }

        public bool AddCustomCategory(Category category)
        {
            DoLoadCheck().Wait();

            lock (syncObject)
            {
                if (categories.Where(c => c.Name.Equals(category.Name)).Count() > 0)
                    return false;

                categories.Add(category);
                ReportingService.ReportCustomCategoryAdded(category.Name);
            }

            SaveCategories();
            return true;
        }

        #endregion




        #region Delete Feed or Category

        public void DeleteFeed(FeedSource feed)
        {
            DoLoadCheck().Wait();

            lock (syncObject)
            {
                feeds.Remove(feed);
            }
        }

        public void DeleteCategory(Category category)
        {
            DoLoadCheck().Wait();

            lock (syncObject)
            {
                categories.Remove(category);
                foreach (var feed in feeds.Where(o => o.Category == category.Name).ToList())
                    feeds.Remove(feed);
            }
        }

        #endregion




        #region Private helper methods

        bool CheckIfCategoryIsEnabledOrIfNoCategory(FeedSource feed)
        {
            if (string.IsNullOrEmpty(feed.Category))
                return true;

            var category = GetCategory(feed.Category);
            if (category == null)
                return true;
            return category.IsEnabled;
        }

        #endregion

        #endregion




        #region Load Feeds and Categories XML files

        List<FeedSource> GetFeedsFromXmlFile()
        {
            var fileName = string.Format("/{0};component/{1}", this.assemblyName, FILE);
            var doc = XDocument.Load(fileName);
            var xmlFeeds = doc.Descendants("Feed")
                .Select(feed =>
                    new FeedSource
                    {
                        Category = feed.Parent.Attribute("Type").ValueOrDefault(),
                        FeedName = feed.Attribute("Name").ValueOrDefault(),
                        FeedUri = feed.ValueOrDefault(),
                        IsEnabled = true,
                        ArticleViewingType = ParseArticleViewingType(feed),
                    })
                .ToList();
            return xmlFeeds;
        }

        ArticleViewingType ParseArticleViewingType(XElement feed)
        {
            var avt = feed.Attribute("ViewType");
            if (avt == null || string.IsNullOrEmpty(avt.Value))
                return ArticleViewingType.Local;

            var type = avt.Value;

            if (type.Equals("mobilizer", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.Mobilizer;

            else if (type.Equals("ie", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.InternetExplorer;

            else
                return ArticleViewingType.Local;
        }

        List<Category> GetCategoriesFromXmlFile()
        {
            var fileName = string.Format("/{0};component/{1}", this.assemblyName, FILE);
            var doc = XDocument.Load(fileName);
            var xmlFeeds = doc.Descendants("Category")
                .Select(o =>
                    new Category
                    {
                        Name = o.Attribute("Type").ValueOrDefault(),
                        IsEnabled = this.canSelectInitialCategories ?
                            false : true
                    })
                .OrderBy(o => o.Name)
                .ToList();
            return xmlFeeds;
        }

        #endregion




        #region Load Feeds and Categories into memory

        public async Task LoadFeeds()
        {
            IEnumerable<FeedSource> isoStorageFeeds = await Get<List<FeedSource>>(FEEDS_KEY);                

            if (isoStorageFeeds == null)
                isoStorageFeeds = new List<FeedSource>();

            var xmlFeeds = GetFeedsFromXmlFile();

            var newFeeds = xmlFeeds.Except(isoStorageFeeds, FeedSourceComparer.Instance);
            var mergedFeedSet = isoStorageFeeds.Union(newFeeds);

            foreach (var feed in newFeeds)
                feed.Id = Guid.NewGuid();

            var deprecatedOrUserAddedFeeds = isoStorageFeeds.Except(xmlFeeds, FeedSourceComparer.Instance);
            foreach (var feed in deprecatedOrUserAddedFeeds)
                feed.IsUserAdded = true;

            foreach (var feed in mergedFeedSet)
                if (feed.Id == new Guid())
                    feed.Id = Guid.NewGuid();

            lock (syncObject)
                feeds = mergedFeedSet.ToList();
        }

        public async Task LoadCategories()
        {
            IEnumerable<Category> isoStorageCategories = await Get<List<Category>>(CATKEY);
                
            if (isoStorageCategories == null)
                isoStorageCategories = new List<Category>();

            var xmlCategories = GetCategoriesFromXmlFile();

            var newCategoriesFromXml = xmlCategories.Except(isoStorageCategories, CategoryComparer.Instance);

            IEnumerable<Category> mergedCategories;

            var deprecatedOrUserAddedCategories = isoStorageCategories.Except(xmlCategories, CategoryComparer.Instance).ToList();//.MemoizeAll();

            if (!this.canAddCategories)
            {
                mergedCategories = isoStorageCategories.Union(newCategoriesFromXml).Except(deprecatedOrUserAddedCategories);
            }
            else
            {
                foreach (var category in deprecatedOrUserAddedCategories)
                    category.UserAdded = true;

                mergedCategories = isoStorageCategories.Union(newCategoriesFromXml);
            }

            lock (syncObject)
                categories = mergedCategories.OrderBy(o => o.Name).ToList();
        }

        #endregion





        #region Save Feeds and Categories to IsoStorage

        public async Task SaveFeeds()
        {
            var feeds = GetAllFeeds();
            await Save<List<FeedSource>>(FEEDS_KEY, feeds);
        }

        public async Task SaveCategories()
        {
            var categories = GetAllCategories();
            await Save<List<Category>>(CATKEY, categories);
        }

        #endregion




        public void SaveOnExit()
        {
            var feedsCopy = GetAllFeeds();
            foreach (var feed in feedsCopy)
            {
                var feedsCategory = GetCategory(feed.Category);
                if (!feed.IsEnabled || (feedsCategory != null && !feedsCategory.IsEnabled))
                    feed.Reset();
            }
            var categoriesCopy = GetAllCategories();
            var news = GetEnabledFeeds().SelectMany(o => o.News).ToList();

            Func<Task> saveOp = async () =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await Save<List<NewsItem>>(NEWS_KEY, news);
                sw.Stop();
                DebugEx.WriteLine("###################  Took {0} ms to write news to disk", sw.ElapsedMilliseconds);
            };

            TaskEx
                .WhenAll(
                    Save<List<FeedSource>>(FEEDS_KEY, feedsCopy),
                    Save<List<Category>>(CATKEY, categoriesCopy),
                    saveOp())//Save<List<NewsItem>>(NEWS_KEY, news))
                .Wait();
        }
    }
}