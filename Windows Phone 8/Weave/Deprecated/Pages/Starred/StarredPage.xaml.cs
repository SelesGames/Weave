using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Controls;
using weave.UI.Advertising;
using System.Reactive.Linq;

namespace weave
{
    public partial class StarredPage : PhoneApplicationPage
    {
        bool isAdCurrentlyEngaged;
        IDisposable subscriptionHandle = null;

        public StarredPage()
        {
            InitializeComponent();
       
            if (DesignerProperties.IsInDesignTool)
                return;
            browser.IsScriptEnabled = true;

            InitializeAdControl();

            Debug.WriteLine("\r\n\r\nMAIN THREAD HAPPENING ON THREAD {0}\r\n\r\n", Thread.CurrentThread.ManagedThreadId);

            StarredNewsItemsService.StarredNewsItemsStream.ObserveOnDispatcher()
                .Subscribe(o => SetListSource(o));

            pivot.SelectionChanged += (s, e) =>
            {
                var selectedNewsItem = pivot.SelectedItem as StarredNewsItem;
                LoadFullHtml(selectedNewsItem);
            };
        }

        private void LoadFullHtml(StarredNewsItem selectedNewsItem)
        {
            if (selectedNewsItem == null)
                return;

            if (subscriptionHandle != null)
                subscriptionHandle.Dispose();

            subscriptionHandle = selectedNewsItem.FullArticleHtmlStream.ObserveOnDispatcher().Subscribe(o =>
                browser.NavigateToString(o));
        }

        void SetListSource(IEnumerable<StarredNewsItem> source)
        {
            var set = source.ToList();
            pivot.ItemsSource = set;
            if (set.Count > 0)
                LoadFullHtml(set.FirstOrDefault());
        }



        #region Ad Control

        void InitializeAdControl()
        {
            if (AdVisibilityService.AreAdsStillBeingShownAtAll)
            {
                adControl.ApplicationId = AdSettings.AdApplicationId;
                adControl.AdUnitId = AdSettings.AdUnits.GetRandomAdUnit();
                //adControl.AdEngaged += (s, e) => isAdCurrentlyEngaged = true;
                //adControl.AdDisengaged += (s, e) => isAdCurrentlyEngaged = false;
            }
            else
            {
                adContainer.Visibility = Visibility.Collapsed;
                adContainer.Children.Remove(adControl);
                adControl = null;
            }
        }

        #endregion



        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            Observable.Start(() =>
            {
                //vm.ScrollPosition = this.currentListBoxScroller.VerticalOffset;
                //vm.SaveTransientState();
            });
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //base.OnBackKeyPress(e);

            if (isAdCurrentlyEngaged)
                return;

            Observable.Start(() =>
            {
                //vm.ScrollPosition = this.currentListBoxScroller.VerticalOffset;
                //vm.SaveTransientState();
                //vm.Dispose();
            });
        }
    }
}