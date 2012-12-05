using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;

namespace weave
{
    public partial class EditSourcePage : PhoneApplicationPage
    {
        EditSourceViewModel viewModel;

        public EditSourcePage()
        {
            InitializeComponent();
            viewModel = new EditSourceViewModel();
            this.DataContext = viewModel;
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition());
            SetValue(RadSlideContinuumAnimation.ApplicationHeaderElementProperty, this.PageTitle);
        }

        void OnPageError()
        {
            MessageBox.Show("There was an error editing the feed");
            try
            {
                NavigationService.GoBack();
            }
            catch { }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                var feedIdString = this.NavigationContext.QueryString["feedId"];
                var feedId = Guid.Parse(feedIdString);
                await viewModel.LoadDataAsync(feedId);
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                OnPageError(); 
            }
        }
    }
}