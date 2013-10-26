using SelesGames;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;
using Weave.ViewModels.Helpers;

namespace weave
{
    public class EditSourceViewModel : INotifyPropertyChanged
    {
        #region private member variables

        const string ARTICLEVIEWMODE_MOBILIZER = "Mobilizer";
        const string ARTICLEVIEWMODE_IE = "Internet Explorer";

        UserInfo user;
        ViewModelLocator viewModelLocator = ServiceResolver.Get<ViewModelLocator>();

        bool suppressShittyJeffWilcoxCode = false;

        // prevents the SelectedArticleViewingMode Property from changing the underlying ArticleViewingMode of the feed
        bool suppressMyOwnShittyCode = false;

        #endregion




        #region Constructor

        public EditSourceViewModel()
        {
            Categories = new ObservableCollection<DisplayableCategory>();
            ArticleViewingModes = new ObservableCollection<string>(new[] { ARTICLEVIEWMODE_MOBILIZER, ARTICLEVIEWMODE_IE });
            IsArticleViewingSelectorEnabled = false;
            user = ServiceResolver.Get<UserInfo>();
        }

        #endregion




        #region Public properties

        public Feed Feed { get; set; }
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

        #endregion




        public void LoadDataAsync(Guid feedId)
        {
            suppressShittyJeffWilcoxCode = true;
            Categories.Clear();
            suppressShittyJeffWilcoxCode = false;

            var feeds = user.Feeds;

            var feed = (Feed)viewModelLocator.Get(feedId.ToString());// feeds.Where(o => o.Id == feedId).SingleOrDefault();
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

        public async Task SaveChanges()
        {
            await user.UpdateFeed(Feed);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}