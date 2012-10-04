using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Clarity.Phone.Extensions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace SelesGames
{
    public enum PopupAction
    {
        UserDismissed,
        Ok
    }

    public static class PopupResult
    {
        public static PopupResult<T> Create<T>(T result, PopupAction action)
        {
            return new PopupResult<T> { Result = result, Action = action };
        }

        public static PopupResult<T> CreateUserDismissed<T>()
        {
            return new PopupResult<T> { Result = default(T), Action = PopupAction.UserDismissed };
        }

        public static PopupResult<T> Create<T>(T result)
        {
            return new PopupResult<T> { Result = result, Action = PopupAction.Ok };
        }
    }

    public class PopupResult<T>
    {
        public PopupAction Action { get; internal set; }
        public T Result { get; internal set; }
    }

    public interface IPopup<T>
    {
        void BeginShow();
        void BeginHide();

        event EventHandler ShowCompleted;
        event EventHandler HideCompleted;
        event EventHandler<EventArgs<PopupResult<T>>> ResultCompleted;
    }

    internal static class extensionMethods
    {
        internal static Panel GetTopMostPanel(this Frame frame)
        {
            var test = frame.GetVisualDescendants().ToList();
            System.Diagnostics.Debug.WriteLine(test);
            var presenters = frame.GetVisualDescendants().OfType<ContentPresenter>();
            for (var i = 0; i < presenters.Count(); i++)
            {

                var panels = presenters.ElementAt(i).GetVisualDescendants().OfType<Panel>();

                if (panels.Any())
                    return panels.FirstOrDefault();
            }
            return null;
        }
    }

    public class PopupService
    {
        internal static UIElement originalContent;
        internal static bool hasDoneMagic = false;
        internal static Panel ovly;

        static PopupService()
        {
            if (!hasDoneMagic)
            {
                var rootVisual = Application.Current.RootVisual as Frame;
                if (rootVisual == null)
                    throw new ArgumentNullException("the rootvisual is not a Frame");

                var border = rootVisual.GetVisualChild(0) as Border;
                var innerElement = border.Child;

                Grid g = new Grid();
                border.Child = g;
                g.Children.Add(innerElement);
                originalContent = innerElement;

                ovly = new Grid
                {
                    Visibility = Visibility.Collapsed,
                };
                g.Children.Add(ovly);

                hasDoneMagic = true;
            }
        }

        public static bool IsOpen { get; internal set; }

        public static void ForceLayout() {}
    }

    public class PopupService<T> : IPopup<T>
    {
        PhoneApplicationPage page;
        IApplicationBar originalAppBar;
        IPopup<T> popup;
        bool isBeginningHide = false;

        public PopupService(IPopup<T> popup)
        {
            this.popup = popup;
            HideOnBackKeyPress = true;
        }

        public event EventHandler<EventArgs<PopupResult<T>>> ResultCompleted;

        public IApplicationBar PopupAppBar { get; set; }
        public bool CloseOnNavigation { get; set; }
        public bool HideOnBackKeyPress { get; set; }

        public event EventHandler ShowCompleted;
        public event EventHandler HideCompleted;




        public void BeginShow()
        {
            InitializePage();
            isBeginningHide = false;
            PopupService.IsOpen = true;

            var child = popup as FrameworkElement;
            if (child == null)
                throw new InvalidCastException("popupElement must derive from FrameworkElement!!");

            RegisterEventHandlers();
            originalAppBar = page.ApplicationBar;

            PopupService.originalContent.IsHitTestVisible = false;
            PopupService.ovly.Children.Add(child);

            Observable.FromEventPattern<EventArgs>(PopupService.ovly, "LayoutUpdated").Take(1).Subscribe(_ =>
            {
                PopupService.ovly.Visibility = Visibility.Visible;

                popup.BeginShow();

                if (page != null)
                {
                    page.ApplicationBar = PopupAppBar;
                }
            });
        }

        void InitializePage()
        {
            if (page != null)
                return;

            var rootVisual = Application.Current.RootVisual as Frame;
            if (rootVisual == null)
                throw new ArgumentNullException("the rootvisual is not a Frame");

            page = rootVisual.GetVisualDescendants().OfType<PhoneApplicationPage>().FirstOrDefault();
            if (page == null)
                throw new ArgumentNullException("the rootvisual does not contain any Page");
        }

        public void BeginHide()
        {
            if (isBeginningHide || !PopupService.IsOpen)
                return;

            isBeginningHide = true;
            ((FrameworkElement)popup).IsHitTestVisible = false;
            popup.BeginHide();
        }




        #region Private event handlers

        void RegisterEventHandlers()
        {
            popup.ShowCompleted += OnShowCompleted;
            popup.HideCompleted += OnHideCompleted;
            popup.ResultCompleted += OnResultCompleted;
            page.BackKeyPress += OnBackKeyPress;
            page.NavigationService.Navigated += OnNavigated;
        }

        void UnregisterEventHandlers()
        {
            popup.ShowCompleted -= OnShowCompleted;
            popup.HideCompleted -= OnHideCompleted;
            popup.ResultCompleted -= OnResultCompleted;
            page.BackKeyPress -= OnBackKeyPress;
            page.NavigationService.Navigated -= OnNavigated;
        }

        void OnShowCompleted(object sender, EventArgs e)
        {
            if (ShowCompleted != null)
                ShowCompleted(this, EventArgs.Empty);
        }

        void OnHideCompleted(object sender, EventArgs e)
        {
            UnregisterEventHandlers();
            ((FrameworkElement)popup).IsHitTestVisible = true;

            PopupService.ovly.Children.Clear();
            PopupService.ovly.Visibility = Visibility.Collapsed;
            PopupService.originalContent.IsHitTestVisible = true;
            PopupService.IsOpen = false;

            page.ApplicationBar = originalAppBar;

            popup = null;
            page = null;
            originalAppBar = null;
            if (HideCompleted != null)
                HideCompleted(this, EventArgs.Empty);
        }

        void OnResultCompleted(object sender, EventArgs<PopupResult<T>> e)
        {
            if (ResultCompleted != null)
                ResultCompleted(this, e);

            BeginHide();
        }

        void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (PopupService.IsOpen && this.HideOnBackKeyPress)
            {
                e.Cancel = true;
                BeginHide();
            }
        }

        void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (CloseOnNavigation)
                BeginHide();
        }

        #endregion


    }
}
