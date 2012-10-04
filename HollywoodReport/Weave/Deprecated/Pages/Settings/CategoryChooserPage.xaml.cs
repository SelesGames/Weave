using System;
using System.Linq;
using System.Reactive.Linq;
using SelesGames;

namespace weave
{
    public partial class CategoryChooserPage : WeavePage
    {
        public CategoryChooserPage()
        {
            InitializeComponent();
        }

        protected override void OnPageLoad(WeaveNavigationEventArgs navigationEventArgs)
        {
            base.OnPageLoad(navigationEventArgs);

            OnPageLoad();
        }

        void OnPageLoad()
        {
            var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
            categoriesListBox.ItemsSource = dal.Feeds.UniqueCategoryNames().OrderBy(o => o);
        }

        void categoriesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (categoriesListBox.SelectedItem != null)
            {
                var selectedCategory = categoriesListBox.SelectedItem as string;
                if (selectedCategory != null)
                {
                    var catName = selectedCategory;
                    EditFeedPage.MementoFeed.Category = catName;
                }
            }
            this.IsHitTestVisible = false;
            Observable.Timer(TimeSpan.FromSeconds(0.3)).ObserveOnDispatcher().Subscribe(notUsed =>
                NavigationService.GoBack());
        }
    }
}