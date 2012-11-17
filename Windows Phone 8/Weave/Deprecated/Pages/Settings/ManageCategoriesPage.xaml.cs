using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using SelesGames;

namespace weave
{
    public partial class ManageCategoriesPage : WeavePage
    {
        bool isVirginal = true;
        public ObservableCollection<Category> Categories { get; private set; }

        public ManageCategoriesPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.AppName.ToUpper();

            //this.GetLayoutUpdated().Take(1).Subscribe(_ => OnPageLoad());
            //GlobalNavigationService.CurrentFrame.OnPageLoad(this, OnPageLoad);
        }

        protected override void OnPageLoad(WeaveNavigationEventArgs navigationEventArgs)
        {
            base.OnPageLoad(navigationEventArgs);
            OnPageLoad();
        }

        void OnPageLoad()
        {
            Categories = new ObservableCollection<Category>();
            DataContext = this;

            var o = ServiceResolver.Get<IStartupTask>();
            o.StartupComplete.Delay(TimeSpan.FromMilliseconds(1)).ObserveOnDispatcher().Subscribe(__ =>
            {
                var dal = ServiceResolver.Get<Data.DataAccessLayer>();
                var categories = dal.GetAllCategories().OrderBy(c => c.Name).ToList();
                categories.IntroducePeriod(TimeSpan.FromMilliseconds(50))
                    .ObserveOnDispatcher()
                    .Subscribe(Categories.Add);
                //var disp = Categories.AddPeriodically(categories, TimeSpan.FromMilliseconds(50), Scheduler.Dispatcher);
            });
        }

        void DeleteButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var category = (sender as Button).DataContext as Category;
            if (category == null)
                return;

            var result = MessageBox.Show(string.Format(
                "Are you sure you want to delete {0}?", category.Name), "Confirm delete", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                Categories.Remove(category);

                var dal = ServiceResolver.Get<Data.DataAccessLayer>();
                dal.DeleteCategory(category);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (isVirginal)
                return;

            //FeedsSettingsService.SaveCategories();
            //FeedsSettingsService.SaveFeeds();

            var dal = ServiceResolver.Get<Data.DataAccessLayer>();
            dal.RefreshAllFeeds();
            AppSettings.TombstoneState.MainPageCurrentPageShouldBeFlushed = true;
        }

        void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            isVirginal = false;
        }
    }
}