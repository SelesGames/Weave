using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace weave
{
    public partial class ChangelogAndComingSoonPage : WeavePage, IDisposable
    {
        readonly string changeLogUri = "http://weavestorage.blob.core.windows.net/settings/changelog.txt";
        readonly string upcomingUri = "http://weavestorage.blob.core.windows.net/settings/upcoming.txt";

        CompositeDisposable disposables = new CompositeDisposable();

        public ChangelogAndComingSoonPage()
        {
            InitializeComponent();

            this.pivot.Title = AppSettings.Instance.AppName.ToUpper();

            this.progressBar.Hide();
        }

        protected override void OnPageLoad(WeaveNavigationEventArgs navigationEventArgs)
        {
            base.OnPageLoad(navigationEventArgs);

            BeginGetChangeLog();

            Observable.FromEventPattern<SelectionChangedEventArgs>(this.pivot, "SelectionChanged")
                .Where(o => o.EventArgs.AddedItems != null && o.EventArgs.AddedItems.Count > 0)
                .Select(o => o.EventArgs.AddedItems[0])
                .OfType<PivotItem>()
                .Where(o => o.Header != null && o.Header.ToString() == "upcoming")
                .Take(1)
                .Subscribe(BeginGetUpcomingFeatures)
                .DisposeWith(this.disposables);
        }

        void BeginGetChangeLog()
        {
            this.progressBar.Show();

            (this.changeLogUri + "?id=" + Guid.NewGuid().ToString()).GetHttpWebResponseAsync()
                .Where(o => o.StatusCode == HttpStatusCode.OK)
                .Select(o => o.GetResponseStreamAsString(System.Text.Encoding.Unicode))
                .ObserveOnDispatcher()
                .Select(System.Windows.Markup.XamlReader.Load)
                .OfType<FrameworkElement>()
                .Subscribe(HandleGetChangeLogResponse, OnGetChangeLogException)
                .DisposeWith(this.disposables);
        }

        void HandleGetChangeLogResponse(FrameworkElement changeLogXaml)
        {
            this.progressBar.Hide();
            this.changeLogScroller.Content = changeLogXaml;
        }

        //void HandleGetChangeLogResponse(string changeLog)
        //{
        //    this.progressBar.Hide();
        //    using (var sr = new StringReader(changeLog))
        //    {
        //        this.changeLogList.ItemsSource = sr.ReadLines();
        //    }
        //}

        void OnGetChangeLogException(Exception ex)
        {
            this.progressBar.Hide();
            DebugEx.WriteLine(ex.ToString());
            //this.changeLogText.Text = "There was an error downloading the latest changelog.  Please try again later.";
        }

        void BeginGetUpcomingFeatures()
        {
            (this.upcomingUri + "?id=" + Guid.NewGuid().ToString()).GetHttpWebResponseAsync()
                .Where(o => o.StatusCode == HttpStatusCode.OK)
                .Select(o => o.GetResponseStreamAsString(System.Text.Encoding.Unicode))
                .ObserveOnDispatcher()
                .Select(System.Windows.Markup.XamlReader.Load)
                .OfType<FrameworkElement>()
                .Subscribe(HandleGetUpcomingFeaturesResponse, OnGetUpcomingFeaturesException)
                .DisposeWith(this.disposables);
        }

        void HandleGetUpcomingFeaturesResponse(FrameworkElement upcomingXaml)
        {
            this.progressBar.Hide();
            this.upcomingScroller.Content = upcomingXaml;
        }

        void OnGetUpcomingFeaturesException(Exception ex)
        {
            this.progressBar.Hide();
            DebugEx.WriteLine(ex.ToString());
        }



        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            Dispose();
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }
    }
}