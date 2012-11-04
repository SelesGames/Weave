using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using weave.Services;

namespace weave
{
    public partial class BackgroundSelectorPage : PhoneApplicationPage
    {
        bool isInPreviewState = false;

        public BackgroundSelectorPage()
        {
            InitializeComponent();
            this.ApplicationTitle.Text = AppSettings.AppName.ToUpper();
            this.listBox.ItemsSource = PanoramicBackgroundManagerService.Current.BackgroundForegroundCombos;
            this.listBox.SetBinding(ListBox.SelectedItemProperty, 
                new Binding("SelectedBackgroundSetting") { Source = PanoramicBackgroundManagerService.Current, Mode = BindingMode.TwoWay });
        }

        private void imagePreviewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isInPreviewState)
                return;

            isInPreviewState = true;
            VisualStateManager.GoToState(this, "Preview", true);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (isInPreviewState)
            {
                isInPreviewState = false;
                e.Cancel = true;
                VisualStateManager.GoToState(this, "Collapsed", true);
            }
        }

        private void listBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }
    }
}