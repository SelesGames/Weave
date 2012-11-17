using SelesGames.Phone.Controls;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Telerik.Windows.Controls;

namespace weave
{
    public class CustomFrame : RadPhoneApplicationFrame
    {
        public static CustomFrame Instance;

        bool isLoading;
        FrameworkElement ClientArea;
        //ContentPresenter PageHost;
        UIElement LoadingOverlay;
        RadialBusyIndicator BusyIndicator;
        Storyboard ShowLoadingSB, HideLoadingSB;

        public CustomFrame() : base()
        {
            DefaultStyleKey = typeof(CustomFrame);
            Instance = this;
            HoldNavigationUntilExitTransitionIsFinished = true;
            this.Transition = new RadEmptyTransition();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ClientArea = base.GetTemplateChild("ClientArea") as FrameworkElement;
            //PageHost = base.GetTemplateChild("PageHost") as ContentPresenter;
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
    }
}
