using SelesGames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using weave.Data;
using Weave.FeedLibrary;
using Weave.FeedSearchService;
using Weave.GoogleReader;

namespace weave
{
    public class AddSourceViewModel : INotifyPropertyChanged
    {
        Weave4DataAccessLayer dataRepo;

        public AddSourceViewModel()
        {
            Categories = new ObservableCollection<Category>();
            SearchResults = new ObservableCollection<Source>();
            SearchPrompt = "Search by a topic, or a website name, or even type in the RSS url directly (don't forget the http://)!";
            dataRepo = ServiceResolver.Get<Weave4DataAccessLayer>();
        }




        #region Category list related

        public class Category
        {
            public string Name { get; set; }
            public string Image { get; set; }
        }

        public ObservableCollection<Category> Categories { get; private set; }

        public async Task LoadCategories()
        {
            Categories.Clear();

            var library = ServiceResolver.Get<ExpandedLibrary>();
            var temp = await library.Feeds.Get();
            var categories = temp.UniqueCategoryNames().OrderBy(o => o).Select(o => new Category { Name = o }).ToList();

            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        #endregion




        #region Search related




        #region internal classes for formatting/displaying page data

        public class Source : INotifyPropertyChanged
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string Category { get; set; }
            public ArticleViewingType ViewType { get; set; }
            public bool IsAdded
            {
                get { return isAdded; }
                set { isAdded = value; PropertyChanged.Raise(this, "IsAdded"); }
            }
            bool isAdded;
            public FeedSource Feed { get; set; }

            public override bool Equals(object obj)
            {
                var that = obj as Source;
                if (that == null || this.Url == null)
                    return false;

                return this.Url.Equals(that.Url, StringComparison.OrdinalIgnoreCase);
            }

            public override int GetHashCode()
            {
                return Url != null ? Url.GetHashCode() : -1;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class SearchResultsException : Exception
        {
            public string SearchResultsMessage { get; set; }
        }

        #endregion




        public ObservableCollection<Source> SearchResults { get; private set; }
        public string SearchString { get; set; }
        public string SearchPrompt
        {
            get { return searchPrompt; }
            set { searchPrompt = value; PropertyChanged.Raise(this, "SearchPrompt"); }
        }
        string searchPrompt;


        public async Task LoadSearchResults()
        {
            if (string.IsNullOrEmpty(SearchString))
                return;

            SearchPrompt = "Searching...";
            SearchResults.Clear();

            var searchService = new FeedSearchService();
            var response = await searchService.SearchForFeedsMatching(SearchString, CancellationToken.None);


            if (response.responseStatus == "200")
            {
                SearchPrompt = null;

                // parse the results from the web query
                var sources = response.responseData.entries.Select(Parse).ToList();

                // load the users enabled feeds that match the search string
                var dataRepo = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
                var enabledFeeds = await dataRepo.Feeds.Get();
                var enabledSources = enabledFeeds
                    .Where(o => o.FeedName.IndexOf(SearchString, StringComparison.OrdinalIgnoreCase) > -1)
                    .Select(ParseEnabledFeed)
                    .ToList();

                // load the feeds from the library that match the search string
                var library = ServiceResolver.Get<ExpandedLibrary>();
                var temp = await library.Feeds.Get();
                var librarySources = temp
                    .Where(o => o.FeedName.IndexOf(SearchString, StringComparison.OrdinalIgnoreCase) > -1)
                    .Select(ParseLibraryFeed)
                    .ToList();

                // the distinct feeds will be a union of all 3
                var distinctSources = enabledSources.Union(librarySources.Union(sources)).Distinct().ToList();

                foreach (var source in distinctSources)//Copy)
                    SearchResults.Add(source);
            }
            else if (response.responseStatus == "999")
            {
                SearchPrompt = "There was an error contacting the search service.  Please make sure you have an internet connection and try again.";
            }
            else
            {
                SearchPrompt = "Google's feed search service is down temporarily.  Please wait a few minutes, then try again.";
            }
        }

        public async Task SaveSearchResults()
        {
            await dataRepo.SaveFeeds();
        }

        public async Task AddFeed(Source source)
        {
            source.IsAdded = true;
            var feed = Parse(source);
            await dataRepo.AddCustomFeed(feed);
            source.Feed = feed;
        }

        public void RemoveFeed(Source source)
        {
            source.IsAdded = false;
            var feed = source.Feed;
            if (feed == null)
                return;

            dataRepo.DeleteFeed(feed);
            source.Feed = null;
        }




        #region Parsing helpers

        Source Parse(Entry entry)
        {
            entry.Sanitize();
            return new Source
            {
                Name = entry.title,
                Url = entry.url,
                ViewType = ArticleViewingType.Mobilizer,
            };
        }

        Source ParseEnabledFeed(FeedSource feed)
        {
            return new Source
            {
                Name = feed.FeedName,
                Url = feed.FeedUri,
                Category = feed.Category,
                ViewType = feed.ArticleViewingType,
                IsAdded = true,
                Feed = feed,
            };
        }

        Source ParseLibraryFeed(FeedSource feed)
        {
            return new Source
            {
                Name = feed.FeedName,
                Url = feed.FeedUri,
                Category = feed.Category,
                ViewType = feed.ArticleViewingType,
                IsAdded = false,
            };
        }

        FeedSource Parse(Source source)
        {
            return new FeedSource
            {
                FeedName = source.Name,
                FeedUri = source.Url,
                Category = source.Category,
                ArticleViewingType = source.ViewType,
            };
        }

        #endregion




        #endregion




        #region Import related

        GoogleReader gReaderClient;
        List<Source> gReaderFeeds;

        public string GReaderUserName { get; set; }
        public string GReaderPassword { get; set; }

        public async Task<GoogleReader.AuthenticationResult> AuthenticateGoogleReader()
        {
            gReaderClient = new GoogleReader(GReaderUserName, GReaderPassword);
            await gReaderClient.Authenticate();
            return gReaderClient.AuthResult;
        }

        public async Task LoadGReaderFeeds()
        {
            await gReaderClient.LoadSubscriptionList();
            gReaderFeeds = gReaderClient.Feeds.Select(Parse).ToList();
        }

        public async Task SaveGReaderFeeds()
        {
            if (gReaderFeeds == null || !gReaderFeeds.Any())
                return;

            var dataRepo = ServiceResolver.Get<weave.Data.Weave4DataAccessLayer>();
            var existingFeeds = await dataRepo.Feeds.Get();
            var newFeeds = gReaderFeeds.Select(Parse).Except(existingFeeds).ToList();

            foreach (var feed in newFeeds)
                await dataRepo.AddCustomFeed(feed);

            await dataRepo.SaveFeeds();
        }

        Source Parse(FeedInfo feed)
        {
            string category = null;
            if (feed.Categories != null && feed.Categories.Any())
                category = feed.Categories.First();
            return new Source
            {
                Category = category,
                Name = feed.Name,
                Url = feed.Uri,
                ViewType = ArticleViewingType.Mobilizer,
            };
        }

        #endregion




        public event PropertyChangedEventHandler PropertyChanged;
    }
}