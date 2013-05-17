using Microsoft.Phone.Controls;
using SelesGames;
using SelesGames.Phone;
using System;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using Weave.ViewModels.Helpers;

namespace weave
{
    public partial class EditSourcePage : PhoneApplicationPage
    {
        EditSourceViewModel viewModel;
        ViewModelLocator viewModelLocator = ServiceResolver.Get<ViewModelLocator>();

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                var feedIdString = this.NavigationContext.QueryString["feedId"];
                var feedId = Guid.Parse(feedIdString);
                viewModel.LoadDataAsync(feedId);
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                OnPageError(); 
            }
        }

        protected override async void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            var t = viewModel.SaveChanges();
            if (t.IsCompleted)
                return;
            else
            {
                e.Cancel = true;
                await t;
                if (NavigationService.TryGoBack() == null)
                    viewModelLocator.Pop();
            }
        }
    }
}