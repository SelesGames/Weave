using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Clarity.Phone.Extensions
{
    // this code has been modified from the orginal code
    // from Kevin Marshall's post 
    // http://blogs.claritycon.com/kevinmarshall/2010/10/13/wp7-page-transitions-sample/

    public class DialogService
    {
        public enum AnimationTypes
        {
            Slide,
            SlideHorizontal,
            Swivel,
            SwivelHorizontal
        }


        #region Storyboard XAML

        private const string SlideUpStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.Y)"" 
                                           Storyboard.TargetName=""LayoutRoot"">
                    <EasingDoubleKeyFrame KeyTime=""0"" Value=""150""/>
                    <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
                        <EasingDoubleKeyFrame.EasingFunction>
                            <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                        </EasingDoubleKeyFrame.EasingFunction>
                    </EasingDoubleKeyFrame>
                </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""0"" To=""1"" Duration=""0:0:0.350"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        private const string SlideHorizontalInStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.X)"" 
                                           Storyboard.TargetName=""LayoutRoot"">
                    <EasingDoubleKeyFrame KeyTime=""0"" Value=""-150""/>
                    <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
                        <EasingDoubleKeyFrame.EasingFunction>
                            <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                        </EasingDoubleKeyFrame.EasingFunction>
                    </EasingDoubleKeyFrame>
                </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""0"" To=""1"" Duration=""0:0:0.350"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        private const string SlideHorizontalOutStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.X)"" 
                                           Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""150"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""1"" To=""0"" Duration=""0:0:0.25"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        private const string SlideDownStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.Y)"" 
                                           Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""150"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""1"" To=""0"" Duration=""0:0:0.25"" 
                                 Storyboard.TargetName=""LayoutRoot"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        internal static readonly string SwivelInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation BeginTime=""0:0:0"" Duration=""0"" 
                                Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.CenterOfRotationY)"" 
                                Storyboard.TargetName=""LayoutRoot""
                                To="".5""/>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationX)"" Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-30""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)""
                                            Storyboard.TargetName=""LayoutRoot"">
                <DiscreteDoubleKeyFrame KeyTime=""0"" Value=""1"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        internal static readonly string SwivelOutStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation BeginTime=""0:0:0"" Duration=""0"" 
                                Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.CenterOfRotationY)"" 
                                Storyboard.TargetName=""LayoutRoot""
                                To="".5""/>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationX)"" Storyboard.TargetName=""LayoutRoot"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""45"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)""
                                            Storyboard.TargetName=""LayoutRoot"">
                <DiscreteDoubleKeyFrame KeyTime=""0"" Value=""1"" />
                <DiscreteDoubleKeyFrame KeyTime=""0:0:0.267"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        #endregion


        private Panel popupContainer;
        private Frame rootVisual;
        private PhoneApplicationPage page;
        private IApplicationBar originalAppBar;
        private Panel overlay;
        private Storyboard showStoryboard;
        private Storyboard hideStoryboard;

        public FrameworkElement Child { get; set; }
        public AnimationTypes AnimationType { get; set; }
        public double VerticalOffset { get; set; }
        public Brush BackgroundBrush { get; set; }

        internal IApplicationBar AppBar { get; set; }
        internal bool IsOpen { get; set; }

        public event EventHandler Closed;
        public event EventHandler Opened;

        // set this to prevent the dialog service from closing on back click
        public bool HasPopup { get; set; }

        internal PhoneApplicationPage Page
        {
            get { return page ?? (page = RootVisual.GetVisualDescendants().OfType<PhoneApplicationPage>().FirstOrDefault()); }
        }

        internal Frame RootVisual
        {
            get { return rootVisual ?? (rootVisual = Application.Current.RootVisual as Frame); }
        }

        static Panel GetTopMostPanel(Frame frame)
        {
            var presenters = frame.GetVisualDescendants().OfType<ContentPresenter>();
            for (var i = 0; i < presenters.Count(); i++)
            {

                var panels = presenters.ElementAt(i).GetVisualDescendants().OfType<Panel>();

                if (panels.Any())
                    return panels.FirstOrDefault();
            }
            return null;
        }

        internal Panel PopupContainer
        {
            get
            {
                if (popupContainer == null)
                {
                    this.popupContainer = GetTopMostPanel(this.rootVisual);
                    //var presenters = RootVisual.GetVisualDescendants().OfType<ContentPresenter>();
                    //for (var i = 0; i < presenters.Count(); i++)
                    //{

                    //    var panels = presenters.ElementAt(i).GetVisualDescendants().OfType<Panel>();

                    //    //if (panels.Count() <= 0)
                    //    //    continue;
                    //    if ( )
                    //    popupContainer = panels.FirstOrDefault();
                    //    break;
                    //}
                }


                return popupContainer;
            }
        }

        public DialogService()
        {
            AnimationType = AnimationTypes.Slide;
        }

        private void InitializePopup()
        {
            // Add overlay which is the size of RootVisual
            overlay = new Grid();

            Grid.SetColumnSpan(overlay, int.MaxValue);
            Grid.SetRowSpan(overlay, int.MaxValue);

            switch (AnimationType)
            {
                case AnimationTypes.SlideHorizontal:
                    showStoryboard = XamlReader.Load(SlideHorizontalInStoryboard) as Storyboard;
                    hideStoryboard = XamlReader.Load(SlideHorizontalOutStoryboard) as Storyboard;
                    overlay.RenderTransform = new TranslateTransform();
                    break;

                case AnimationTypes.Slide:
                    showStoryboard = XamlReader.Load(SlideUpStoryboard) as Storyboard;
                    hideStoryboard = XamlReader.Load(SlideDownStoryboard) as Storyboard;
                    overlay.RenderTransform = new TranslateTransform();
                    break;

                default:
                    showStoryboard = XamlReader.Load(SwivelInStoryboard) as Storyboard;
                    hideStoryboard = XamlReader.Load(SwivelOutStoryboard) as Storyboard;
                    overlay.Projection = new PlaneProjection();
                    break;
            }

            overlay.Children.Add(Child);

            if (BackgroundBrush != null)
                overlay.Background = BackgroundBrush;

            overlay.Margin = new Thickness(0, VerticalOffset, 0, 0);
            overlay.Opacity = 0;

            // Initialize popup to draw the context menu over all controls
            if (PopupContainer != null && PopupContainer.Children != null)
                PopupContainer.Children.Add(overlay);
        }

        protected internal void SetAlignmentsOnOverlay(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            if (overlay != null)
            {
                overlay.HorizontalAlignment = horizontalAlignment;
                overlay.VerticalAlignment = verticalAlignment;
            }
        }

        /// <summary>
        /// Shows the context menu.
        /// </summary>
        public void Show()
        {
            IsOpen = true;

            InitializePopup();

            Page.BackKeyPress += OnBackKeyPress;
            //Page.NavigationService.Navigated += OnNavigated;

            originalAppBar = Page.ApplicationBar;

            Observable.FromEventPattern<EventArgs>(showStoryboard, "Completed").Take(1).Subscribe(notUsed => OnStoryboardCompleted());

            foreach (Timeline t in showStoryboard.Children)
                Storyboard.SetTarget(t, overlay);

            Observable.FromEventPattern<EventArgs>(PopupContainer, "LayoutUpdated").Take(1).Subscribe(notUsed =>
            {
                showStoryboard.Begin();

                if (Page != null)
                {
                    Page.ApplicationBar = AppBar;
                }
            });
        }

        void OnStoryboardCompleted()
        {
            if (Opened != null)
                Opened(this, null);
        }

        //void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    Hide();
        //}

        public void Hide()
        {
            if (!IsOpen)
                return;

            if (Page != null)
            {
                Page.BackKeyPress -= OnBackKeyPress;
                //Page.NavigationService.Navigated -= OnNavigated;

                if (originalAppBar != null)
                    Page.ApplicationBar = originalAppBar;
                else
                    Page.ApplicationBar = null;

                page = null;
            }

            hideStoryboard.Stop();
            foreach (Timeline t in hideStoryboard.Children)
            {
                Storyboard.SetTarget(t, overlay);
            }
            Observable.FromEventPattern<EventArgs>(hideStoryboard, "Completed").Take(1).Subscribe(notUsed => OnHideStoryboardCompleted());
            hideStoryboard.Begin();
        }

        void OnHideStoryboardCompleted()
        {
            IsOpen = false;

            if (PopupContainer != null)
            {
                PopupContainer.Children.Remove(overlay);
            }

            if (null != overlay)
            {
                overlay.Children.Clear();
                overlay = null;
            }

            if (Closed != null)
                Closed(this, null);
        }

        public void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (HasPopup)
            {
                e.Cancel = true;
                return;
            }

            //if (IsOpen)
            //{
            //    e.Cancel = true;
            //    Hide();
            //}
        }
    }
}