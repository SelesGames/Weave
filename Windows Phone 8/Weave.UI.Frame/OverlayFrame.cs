using SelesGames.Phone.Controls;
using System.Windows;
using System.Windows.Media.Animation;
using Telerik.Windows.Controls;

namespace Weave.UI.Frame
{
    public class OverlayFrame : RadPhoneApplicationFrame
    {
        bool isLoading;
        FrameworkElement ClientArea;

        UIElement LoadingOverlay;
        RadialBusyIndicator BusyIndicator;
        Storyboard ShowLoadingSB, HideLoadingSB;

        public OverlayFrame() : base()
        {
            DefaultStyleKey = typeof(OverlayFrame);
            HoldNavigationUntilExitTransitionIsFinished = true;
            this.Transition = new RadEmptyTransition();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ClientArea = base.GetTemplateChild("ClientArea") as FrameworkElement;
            LoadingOverlay = base.GetTemplateChild("LoadingOverlay") as UIElement;
            BusyIndicator = base.GetTemplateChild("BusyIndicator") as RadialBusyIndicator;
            ShowLoadingSB = ClientArea.Resources["ShowLoadingSB"] as Storyboard;
            HideLoadingSB = ClientArea.Resources["HideLoadingSB"] as Storyboard;

            if (Content != null)
            {
                OnContentChanged(null, Content);
            }

            if (IsLoading)
            {
                ShowLoadingIndicator();
            }
            else
            {
                BusyIndicator.IsPlaying = false;
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    if (isLoading)
                        ShowLoadingIndicator();
                    else
                        HideLoadingIndicator();
                }
            }
        }

        void ShowLoadingIndicator()
        {
            if (ShowLoadingSB == null)
                return;

            ShowLoadingSB.Begin();
            BusyIndicator.IsPlaying = true;
        }

        async void HideLoadingIndicator()
        {
            if (HideLoadingSB == null)
                return;

            await HideLoadingSB.BeginWithNotification().ToTask();
            BusyIndicator.IsPlaying = false;
        }




        #region Dependency Properties

        public static readonly DependencyProperty OverlayTextProperty =
            DependencyProperty.Register("OverlayText", typeof(string), typeof(OverlayFrame), new PropertyMetadata("Loading news from memory"));

        public string OverlayText
        {
            get { return (string)GetValue(OverlayTextProperty); }
            set { SetValue(OverlayTextProperty, value); }
        }

        #endregion
    }
}
