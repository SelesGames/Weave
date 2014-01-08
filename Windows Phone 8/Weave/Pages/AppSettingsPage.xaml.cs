using Microsoft.Phone.Controls;
using SelesGames;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Weave.Customizability;
using Weave.SavedState;
using Weave.Settings;
using Weave.ViewModels;
using Windows.Phone.System.UserProfile;

namespace weave
{
    public partial class AppSettingsPage : PhoneApplicationPage, IDisposable
    {
        PermanentState permState;
        SpeakArticleVoices voices;
        UserInfo user;
        string entryMarkReadDeleteTime, entryUnreadDeleteTime;

        public AppSettingsPage()
        {
            InitializeComponent();
            voices = Resources["Voices"] as SpeakArticleVoices;

            permState = ServiceResolver.Get<PermanentState>();
            voicesList.SelectedItem = voices.GetByDisplayName(permState.SpeakTextVoice);

            articleListToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsHideAppBarOnArticleListPageEnabled") { Source = permState, Mode = BindingMode.TwoWay });
            articleViewerToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsHideAppBarOnArticleViewerPageEnabled") { Source = permState, Mode = BindingMode.TwoWay });
            systemTrayToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsSystemTrayVisibleWhenPossible") { Source = permState, Mode = BindingMode.TwoWay });
            
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumTransition());
            SetValue(RadSlideContinuumAnimation.ApplicationHeaderElementProperty, this.PageTitle);

            user = ServiceResolver.Get<UserInfo>();

            markedReadList.SelectedItem = entryMarkReadDeleteTime = user.ArticleDeletionTimeForMarkedRead;
            unreadList.SelectedItem = entryUnreadDeleteTime = user.ArticleDeletionTimeForUnread;

            markedReadList.SelectionChanged += OnMarkedReadSelectionChanged;
            unreadList.SelectionChanged += OnUnreadSelectionChanged;
            voicesList.SelectionChanged += OnVoicesListSelectionChanged;
            enableLockScreenButton.Tap += OnRequestLiveLockScreenButtonTapped;
            Loaded += OnPageLoaded;
        }

        async Task SaveChanges()
        {
            if (user.ArticleDeletionTimeForMarkedRead != entryMarkReadDeleteTime ||
                user.ArticleDeletionTimeForUnread != entryUnreadDeleteTime)
            {
                await user.SetArticleDeleteTimes();
            }
        }




        #region Event Handlers

        void OnMarkedReadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.OfType<ArticleDeleteTime>().FirstOrDefault();
            if (selected == null)
                return;

            user.ArticleDeletionTimeForMarkedRead = selected.Display;
        }

        void OnUnreadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.OfType<ArticleDeleteTime>().FirstOrDefault();
            if (selected == null)
                return;

            user.ArticleDeletionTimeForUnread = selected.Display;
        }

        void OnVoicesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.OfType<SpeakArticleVoice>().FirstOrDefault();
            if (selected == null)
                return;

            permState.SpeakTextVoice = selected.DisplayName;
        }

        async void OnRequestLiveLockScreenButtonTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                if (!LockScreenManager.IsProvidedByCurrentApplication)
                {
                    var result = await LockScreenManager.RequestAccessAsync();
                    if (result == LockScreenRequestResult.Denied)
                        return;

                    MessageBox.Show(
                        "Congratulations - the next time your Live Tile is updated, your Lock Screen will be updated too!",
                        "Lock Screen updating enabled",
                        MessageBoxButton.OK);
                    LockScreen.SetImageUri(
                        new Uri("ms-appx:///Assets/Tiles/lock_screen_placeholder.jpg", UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            enableLockScreenButton.IsEnabled = !LockScreenManager.IsProvidedByCurrentApplication;
        }

        #endregion




        public void Dispose()
        {
            markedReadList.SelectionChanged -= OnMarkedReadSelectionChanged;
            unreadList.SelectionChanged -= OnUnreadSelectionChanged;
            voicesList.SelectionChanged -= OnVoicesListSelectionChanged;
            enableLockScreenButton.Tap -= OnRequestLiveLockScreenButtonTapped;
            Loaded -= OnPageLoaded;
        }
    }
}