using Microsoft.Phone.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Weave.Customizability;
using Windows.Phone.System.UserProfile;

namespace weave
{
    public partial class AppSettingsPage : PhoneApplicationPage, IDisposable
    {
        PermanentState permState;
        ArticleDeleteTimesForMarkedRead markedReadTimes;
        ArticleDeleteTimesForUnread unreadTimes;
        SpeakArticleVoices voices;

        public AppSettingsPage()
        {
            InitializeComponent();
            markedReadTimes = Resources["MarkedReadTimes"] as ArticleDeleteTimesForMarkedRead;
            unreadTimes = Resources["UnreadTimes"] as ArticleDeleteTimesForUnread;
            voices = Resources["Voices"] as SpeakArticleVoices;

            permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();
            markedReadList.SelectedItem = markedReadTimes.GetByDisplayName(permState.ArticleDeletionTimeForMarkedRead);
            unreadList.SelectedItem = unreadTimes.GetByDisplayName(permState.ArticleDeletionTimeForUnread);
            voicesList.SelectedItem = voices.GetByDisplayName(permState.SpeakTextVoice);

            articleListToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsHideAppBarOnArticleListPageEnabled") { Source = permState, Mode = BindingMode.TwoWay });
            articleViewerToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsHideAppBarOnArticleViewerPageEnabled") { Source = permState, Mode = BindingMode.TwoWay });
            systemTrayToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsSystemTrayVisibleWhenPossible") { Source = permState, Mode = BindingMode.TwoWay });
            
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumTransition());
            SetValue(RadSlideContinuumAnimation.ApplicationHeaderElementProperty, this.PageTitle);

            markedReadList.SelectionChanged += OnMarkedReadSelectionChanged;
            unreadList.SelectionChanged += OnUnreadSelectionChanged;
            voicesList.SelectionChanged += OnVoicesListSelectionChanged;
            enableLockScreenButton.Tap += OnRequestLiveLockScreenButtonTapped;
            Loaded += OnPageLoaded;
        }




        #region Event Handlers

        void OnMarkedReadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.OfType<ArticleDeleteTime>().FirstOrDefault();
            if (selected == null)
                return;

            permState.ArticleDeletionTimeForMarkedRead = selected.Display;
        }

        void OnUnreadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.OfType<ArticleDeleteTime>().FirstOrDefault();
            if (selected == null)
                return;

            permState.ArticleDeletionTimeForUnread = selected.Display;
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