using Microsoft.Phone.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Weave.Customizability;

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

            Loaded += OnLoaded;

            articleListToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsHideAppBarOnArticleListPageEnabled") { Source = permState, Mode = BindingMode.TwoWay });
            articleViewerToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsHideAppBarOnArticleViewerPageEnabled") { Source = permState, Mode = BindingMode.TwoWay });
            systemTrayToggle.SetBinding(ToggleSwitch.IsCheckedProperty, new Binding("IsSystemTrayVisibleWhenPossible") { Source = permState, Mode = BindingMode.TwoWay });

            Binding b = new Binding("ArticleListFormat")
            {
                Converter = new DelegateValueConverter(
                    value =>
                    {
                        var format = (ArticleListFormatType)value;
                        return format == ArticleListFormatType.SmallImage;
                    },
                    value =>
                    {
                        var boolValue = (bool)value;
                        return boolValue ? ArticleListFormatType.SmallImage : ArticleListFormatType.BigImage;
                    }),
                Source = permState,
                Mode = BindingMode.TwoWay,
            };
            classicArticleListToggle.SetBinding(ToggleSwitch.IsCheckedProperty, b);
            
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumTransition());
            SetValue(RadSlideContinuumAnimation.ApplicationHeaderElementProperty, this.PageTitle);
        }




        #region hook up events on loaded and unhook on unloaded

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Unloaded += OnUnloaded;
            markedReadList.SelectionChanged += OnMarkedReadSelectionChanged;
            unreadList.SelectionChanged += OnUnreadSelectionChanged;
            voicesList.SelectionChanged += OnVoicesListSelectionChanged;
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;
            markedReadList.SelectionChanged -= OnMarkedReadSelectionChanged;
            unreadList.SelectionChanged -= OnUnreadSelectionChanged;
            voicesList.SelectionChanged -= OnVoicesListSelectionChanged;
        }

        #endregion




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

        #endregion




        public void Dispose()
        {
            Loaded -= OnLoaded;
        }
    }
}