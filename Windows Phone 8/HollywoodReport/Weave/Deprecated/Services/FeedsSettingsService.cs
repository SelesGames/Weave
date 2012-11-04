using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace weave
{
    public static class FeedsSettingsService
    {
        const string FILE = "Feeds.xml";
        static List<Category> categories;
        static List<FeedSource> feeds;
        static object syncObject = new object();
        //static Category allNewsCategory;

        public static List<Category> GetAllCategories()
        {
            List<Category> categoriesCopy;
            lock (syncObject)
                categoriesCopy = categories.ToList();

            return categoriesCopy;
        }

        public static List<FeedSource> GetAllFeeds()
        { 
            List<FeedSource> feedsCopy;
            lock (syncObject)
                feedsCopy = feeds.ToList();
            return feedsCopy; 
        }

        public static IEnumerable<Category> EnabledCategories
        {
            get { return GetAllCategories().Where(o => o.IsEnabled); }
        }

        //public static IEnumerable<Category> EnabledCategories { get; private set; }

        public static Category GetCategory(string categoryName)
        {
            return GetAllCategories().SingleOrDefault(category => category.Name == categoryName);
        }




        #region Load Feeds and Categories XML files

        static List<FeedSource> GetFeedsFromXmlFile()
        {
            var fileName = string.Format("/{0};component/{1}", AppSettings.AssemblyName, FILE);
            var doc = XDocument.Load(fileName);
            var xmlFeeds = doc.Descendants("Feed")
                .Select(feed =>
                    new FeedSource
                    {
                        Category = feed.Parent.Attribute("Type").ValueOrDefault(),
                        FeedName = feed.Attribute("Name").ValueOrDefault(),
                        FeedUri = feed.ValueOrDefault(),
                        IsEnabled = true,
                    })
                .ToList();
            return xmlFeeds;
        }

        static List<Category> GetCategoriesFromXmlFile()
        {
            var fileName = string.Format("/{0};component/{1}", AppSettings.AssemblyName, FILE);
            var doc = XDocument.Load(fileName);
            var xmlFeeds = doc.Descendants("Category")
                .Select(o =>
                    new Category
                    {
                        Name = o.Attribute("Type").ValueOrDefault(),
                        IsEnabled = AppSettings.CanSelectInitialCategories ?
                            false : true
                    })
                .OrderBy(o => o.Name)
                .ToList();
            return xmlFeeds;
        }

        #endregion




        #region Load Feeds and Categories into memory

        public static void LoadFeeds()
        {
            IEnumerable<FeedSource> isoStorageFeeds = IsolatedStorageService.GetFeeds();

            if (isoStorageFeeds == null)
                isoStorageFeeds = new List<FeedSource>();

            var xmlFeeds = GetFeedsFromXmlFile();

            var newFeeds = xmlFeeds.Except(isoStorageFeeds, FeedSourceComparer.Instance);
            var mergedFeedSet = isoStorageFeeds.Union(newFeeds);

            var deprecatedOrUserAddedFeeds = isoStorageFeeds.Except(xmlFeeds, FeedSourceComparer.Instance);
            foreach (var feed in deprecatedOrUserAddedFeeds)
                feed.IsUserAdded = true;

            //if (!AppSettings.CanAddCategories)
            //{
            //    var groupedFeeds = mergedFeedSet.GroupBy(feed => feed.Category).ToList();
            //    var toEliminate = groupedFeeds.Where(o => categories.Where(c => c.Name == o.Key).Count() == 0).SelectMany(o => o).ToList();
            //    mergedFeedSet = mergedFeedSet.Except(toEliminate);
            //}

            lock (syncObject)
                feeds = mergedFeedSet.ToList();

            //var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //var categoryLookup = categories.ToLookup(o => o.Name);
            //foreach (var feed in feeds)
            //{
            //    var cat = categoryLookup[feed.Category].FirstOrDefault();
            //    if (cat != null)
            //        cat.Feeds.Add(feed);
            //    allNewsCategory.Feeds.Add(feed);
            //}
            //stopwatch.Stop();
            //DebugEx.WriteLine("Took {0} ms to organize feeds into categories", stopwatch.ElapsedMilliseconds);
        }

        public static void LoadCategories()
        {
            IEnumerable<Category> isoStorageCategories = IsolatedStorageService.GetCategories();

            if (isoStorageCategories == null)
                isoStorageCategories = new List<Category>();

            var xmlCategories = GetCategoriesFromXmlFile();

            var newCategoriesFromXml = xmlCategories.Except(isoStorageCategories, CategoryComparer.Instance);

            IEnumerable<Category> mergedCategories;

            var deprecatedOrUserAddedCategories = isoStorageCategories.Except(xmlCategories, CategoryComparer.Instance).Memoize();

            if (!AppSettings.CanAddCategories)
            {
                //var categoriesToDeleteFromXml = isoStorageCategories.Except(xmlCategories, CategoryComparer.Instance);
                mergedCategories = isoStorageCategories.Union(newCategoriesFromXml).Except(deprecatedOrUserAddedCategories);
            }
            else
            {
                foreach (var category in deprecatedOrUserAddedCategories)
                    category.UserAdded = true;

                mergedCategories = isoStorageCategories.Union(newCategoriesFromXml);
            }


            lock(syncObject)
                categories = mergedCategories.OrderBy(o => o.Name).ToList();

            //var temp = categories.Where(o => o.IsEnabled).ToList();
            //allNewsCategory = new Category { IsEnabled = true, Name = "All News", UserAdded = false };
            //temp.Insert(0, allNewsCategory);
            //EnabledCategories = temp;
        }

        #endregion




        #region Add Feed or Category

        public static bool AddCustomFeed(FeedSource newFeed)
        {
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
            newFeed.CheckForAndGetNewFeedData();
            SaveFeeds();
            return true;
        }

        public static bool AddCustomCategory(Category category)
        {
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

        public static void DeleteFeed(FeedSource feed)
        {
            lock (syncObject)
            {
                feeds.Remove(feed);
            }
        }

        public static void DeleteCategory(Category category)
        {
            lock (syncObject)
            {
                categories.Remove(category);
                foreach (var feed in feeds.Where(o => o.Category == category.Name).ToList())
                    feeds.Remove(feed);
            }
        }

        #endregion




        #region Save Feeds and Categories to IsoStorage

        public static void SaveFeeds()
        {
            //Observable.Start(() => IsolatedStorageService.SaveFeedsToIsoStorage(GetAllFeeds()), OneThreadScheduler.Instance);
            Observable.Start(() => IsolatedStorageService.SaveFeedsToIsoStorage(GetAllFeeds()));
        }

        public static void SaveCategories()
        {
            //Observable.Start(() => IsolatedStorageService.SaveCategoriesToIsoStorage(GetAllCategories()), OneThreadScheduler.Instance);
            Observable.Start(() => IsolatedStorageService.SaveCategoriesToIsoStorage(GetAllCategories()));
        }

        #endregion




        public static void SaveOnExit()
        {
            //var enabledFeeds = NewsItemService.GetEnabledFeeds();
            //foreach (var feed in enabledFeeds)
            //    feed.SaveNews();

            var feedsCopy = GetAllFeeds();
            foreach (var feed in feedsCopy)
            {
                var feedsCategory = GetCategory(feed.Category);
                if (!feed.IsEnabled || (feedsCategory != null && !feedsCategory.IsEnabled))
                    feed.Reset();
            }
            var categoriesCopy = GetAllCategories();
            IsolatedStorageService.SaveFeedsToIsoStorage(feedsCopy);
            IsolatedStorageService.SaveCategoriesToIsoStorage(categoriesCopy);
        }
    }
}
