using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// This class performs animation between pages.
    /// </summary>
    [TemplatePart(Name = AnimatingFrame.OldTemplatePart, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = AnimatingFrame.NewTemplatePart, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = AnimatingFrame.ClientAreaPart, Type = typeof(FrameworkElement))]
    public class AnimatingFrame : PhoneApplicationFrame
    {

        /// <summary>
        /// Whether or not frame animation is enabled
        /// </summary>
        public static readonly DependencyProperty IsAnimationEnabledProperty = DependencyProperty.Register("IsAnimationEnabled", typeof(bool), typeof(AnimatingFrame), new PropertyMetadata(true));
        public bool IsAnimationEnabled
        {
            get { return (bool)GetValue(IsAnimationEnabledProperty); }
            set { SetValue(IsAnimationEnabledProperty, value); }
        }

        /// <summary>
        /// Name of the visual state that represents the initial state of a forward navigation
        /// </summary>
        public static readonly DependencyProperty BeforeLoadStateProperty = DependencyProperty.Register("BeforeLoadState", typeof(string), typeof(AnimatingFrame), new PropertyMetadata("BeforeLoad"));
        public string BeforeLoadState
        {
            get { return (string)GetValue(BeforeLoadStateProperty); }
            set { SetValue(BeforeLoadStateProperty, value); }
        }

        /// <summary>
        /// Name of the visual state that represents the final state of a forward or backward navigation
        /// </summary>
        public static readonly DependencyProperty LoadedStateProperty = DependencyProperty.Register("LoadedState", typeof(string), typeof(AnimatingFrame), new PropertyMetadata("Loaded"));
        public string LoadedState
        {
            get { return (string)GetValue(LoadedStateProperty); }
            set { SetValue(LoadedStateProperty, value); }
        }

        /// <summary>
        /// Name of the visual state that represents the initial state of a backward navigation
        /// </summary>
        public static readonly DependencyProperty AfterLoadStateProperty = DependencyProperty.Register("AfterLoadState", typeof(string), typeof(AnimatingFrame), new PropertyMetadata("AfterLoad"));
        public string AfterLoadState
        {
            get { return (string)GetValue(AfterLoadStateProperty); }
            set { SetValue(AfterLoadStateProperty, value); }
        }

        bool isForward = false;
        ContentPresenter oldContentPresenter;
        ContentPresenter newContentPresenter;
        VisualStateGroup visualStateGroup;

        public AnimatingFrame()
        {
            DefaultStyleKey = typeof(AnimatingFrame);

            // MIX10: Note that a bug prevents this from being called when you go back with the
            // hardware key, so you will always see the 'forward' navigation if you use the
            // hardware key; it will also crash if you go back while still animating forwards
            this.Navigating += (s, e) =>
            {
                isForward = (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back);
                if (oldContentPresenter != null)
                    oldContentPresenter.Content = null;
            };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            oldContentPresenter = GetTemplateChild(OldTemplatePart) as ContentPresenter;
            newContentPresenter = GetTemplateChild(NewTemplatePart) as ContentPresenter;

            // Remove old handlers, if any
            if (visualStateGroup != null)
                visualStateGroup.CurrentStateChanged -= OnStateChanged;

            // Add new handlers
            visualStateGroup = GetTemplateChild("AnimationStates") as VisualStateGroup;
            if (visualStateGroup != null)
                visualStateGroup.CurrentStateChanged += OnStateChanged;

            if (Content != null)
                OnContentChanged(null, Content);
        }

        /// <summary>
        /// Removes the old content from the tree once the animation is over
        /// </summary>
        private void OnStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "Loaded" && oldContentPresenter != null)
                oldContentPresenter.Content = null;
        }

        /// <summary>
        /// Switches content and animates the two content presenters whenever the content
        /// of the control is updated (ie, it is navigated).
        /// </summary>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (newContentPresenter == null || oldContentPresenter == null)
                return;

            if (IsAnimationEnabled != true)
            {
                oldContentPresenter.Content = null;
                newContentPresenter.Content = newContent;
                return;
            }

            newContentPresenter.Content = newContent;
            oldContentPresenter.Content = oldContent;

            string startingState = AfterLoadState;
            if (isForward)
                startingState = BeforeLoadState;

            // Attempt to switch states
            if (VisualStateManager.GoToState(this, startingState, false))
                if (VisualStateManager.GoToState(this, LoadedState, true))
                    return;

            // Switching states failed; just hide the old content
            oldContentPresenter.Content = null;
        }

        public const string OldTemplatePart = "OldContent";
        public const string NewTemplatePart = "NewContent";
        public const string ClientAreaPart = "ClientArea";
    }
}
