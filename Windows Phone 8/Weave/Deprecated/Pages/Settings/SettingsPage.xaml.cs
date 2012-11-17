using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace weave
{
    public partial class SettingsPage : PhoneApplicationPage, IDisposable
    {
        CompositeDisposable disposables = new CompositeDisposable();

        public SettingsPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.Instance.AppName.ToUpper();

            if (!AppSettings.Instance.CanManageFeeds) manageFeedsButton.Visibility = Visibility.Collapsed;
            //if (!weave.Services.PanoramicBackgroundManagerService.Current.CanManuallyChooseBackground) changePanoBGButton.Visibility = Visibility.Collapsed;

            manageFeedsButton.GetClick().Subscribe(GlobalNavigationService.ToManageSourcesPage).DisposeWith(this.disposables);
            infoAndSupportButton.GetClick().Subscribe(GlobalNavigationService.ToInfoAndSupportPage).DisposeWith(this.disposables);
            viewChangeLogButton.GetClick().Subscribe(GlobalNavigationService.ToChangeLogAndComingSoonPage).DisposeWith(this.disposables);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            this.Dispose();
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }
    }
}