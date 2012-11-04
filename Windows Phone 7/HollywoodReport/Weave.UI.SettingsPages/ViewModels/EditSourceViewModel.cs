using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using SelesGames;

namespace weave
{
    public class EditSourceViewModel : INotifyPropertyChanged
    {
        const string ARTICLEVIEWMODE_MOBILIZER = "Mobilizer";
        const string ARTICLEVIEWMODE_IE = "Internet Explorer";


        bool suppressShittyJeffWilcoxCode = false;

        // prevents the SelectedArticleViewingMode Property from changing the underlying ArticleViewingMode of the feed
        bool suppressMyOwnShittyCode = false;

        public FeedSource Feed { get; set; }
        public ObservableCollection<DisplayableCategory> Categories { get; private set; }
        public ObservableCollection<string> ArticleViewingModes { get; private set; }

        DisplayableCategory selectedCategory;
        public DisplayableCategory SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                if (suppressShittyJeffWilcoxCode)
                    return;

                selectedCategory = value;
                PropertyChanged.Raise(this, "SelectedCategory");
                if (value == null || value == DisplayableCategory.NONE)
                    Feed.Category = null;
                else
                    Feed.Category = value.Name;
            }
        }

        string selectedArticleViewingMode;
        public string SelectedArticleViewingMode
        {
            get { return selectedArticleViewingMode; }
            set
            {
                selectedArticleViewingMode = value;
                PropertyChanged.Raise(this, "SelectedArticleViewingMode");
                if (value != null && !suppressMyOwnShittyCode)
                {
                    if (value == ARTICLEVIEWMODE_MOBILIZER)
                        Feed.ArticleViewingType = ArticleViewingType.Mobilizer;
                    else if (value == ARTICLEVIEWMODE_IE)
                        Feed.ArticleViewingType = ArticleViewingType.InternetExplorer;
                }
            }
        }

        bool isArticleViewingSelectorEnabled;
        public bool IsArticleViewingSelectorEnabled
        {
            get { return isArticleViewingSelectorEnabled; }
            set { isArticleViewingSelectorEnabled = value; PropertyChanged.Raise(this, "IsArticleViewingSelectorEnabled"); }
        }

        public EditSourceViewModel()
        {
            Categories = new ObservableCollection<DisplayableCategory>();
            ArticleViewingModes = new ObservableCollection<string>(new[] { ARTICLEVIEWMODE_MOBILIZER, ARTICLEVIEWMODE_IE});
            IsArticleViewingSelectorEnabled = false;
        }

        public async Task LoadDataAsync(Guid feedId)
        {
            suppressShittyJeffWilcoxCode = true;
            Categories.Clear();
            suppressShittyJeffWilcoxCode = false;

            var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
            var feeds = await dal.Feeds.Get();

            var feed = feeds.Where(o => o.Id == feedId).SingleOrDefault();
            if (feed == null)
                throw new Exception(string.Format("No Feed found with ID {0}", feedId));

            Feed = feed;
            PropertyChanged.Raise(this, "Feed");

            var categories = feeds.UniqueCategoryNames().OrderBy(o => o).OfType<string>();
            var displayableCategories = categories.Select(o => new DisplayableCategory { Name = o, DisplayName = o }).ToList();
            displayableCategories.Insert(0, DisplayableCategory.NONE);

            foreach (var c in displayableCategories)
                Categories.Add(c);

            SelectedCategory = displayableCategories.Where(o => o.Name == feed.Category).SingleOrDefault();

            suppressMyOwnShittyCode = true;

            if (feed.ArticleViewingType == ArticleViewingType.Mobilizer || feed.ArticleViewingType == ArticleViewingType.MobilizerOnly)
                SelectedArticleViewingMode = ARTICLEVIEWMODE_MOBILIZER;

            else if (feed.ArticleViewingType == ArticleViewingType.InternetExplorer || feed.ArticleViewingType == ArticleViewingType.InternetExplorerOnly)
                SelectedArticleViewingMode = ARTICLEVIEWMODE_IE;

            suppressMyOwnShittyCode = false;

            if (feed.ArticleViewingType == ArticleViewingType.InternetExplorerOnly || feed.ArticleViewingType == ArticleViewingType.MobilizerOnly)
                IsArticleViewingSelectorEnabled = false;
            else
                IsArticleViewingSelectorEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DisplayableCategory
    {
        public static DisplayableCategory NONE = new DisplayableCategory { Name = null, DisplayName = "None" };

        public string Name { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
