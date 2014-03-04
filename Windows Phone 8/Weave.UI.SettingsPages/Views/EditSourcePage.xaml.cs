using Microsoft.Phone.Controls;
using SelesGames;
using SelesGames.Phone;
using System;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using Weave.UI.Frame;
using Weave.ViewModels.Helpers;
using SelesGames.Phone;

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

            var frame = ServiceResolver.Get<OverlayFrame>();

            try
            {
                var t = viewModel.SaveChanges();
                if (t.IsCompleted)
                    return;
                else
                {
                    e.Cancel = true;

                    frame.OverlayText = "Saving...";
                    frame.IsLoading = true;

                    await t;
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Unable to save changes to Feed");
            }
            finally
            {
                frame.IsLoading = false;

                if (NavigationService.TryGoBack() == null)
                    viewModelLocator.Pop();
            }
        }
    }
}