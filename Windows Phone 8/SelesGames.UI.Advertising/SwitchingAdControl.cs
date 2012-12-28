using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SelesGames.UI.Advertising
{
    public class SwitchingAdControl : Control, IDisposable
    {
        Panel AdContainer;
        Storyboard OnNewAdSB;
        IAdControlAdapter adControl;
        AdControlFactory factory;
        string keywords;
        bool isHidden = false;
        int currentFaultLevel = 0;
        bool isDisposed = false;
        readonly TimeSpan INITIAL_AD_DISPLAY_DELAY = TimeSpan.FromSeconds(3);

        public bool PlayAnimations { get; set; }
        public bool IsAdCurrentlyEngaged { get { return false; } }
        public int MaxFaultLevel { get; set; }

        public SwitchingAdControl(AdControlFactory factory, string keywords)
            : this(factory)
        {
            this.factory = factory;
            this.keywords = keywords;
        }

        public SwitchingAdControl(AdControlFactory factory)
        {
            DefaultStyleKey = typeof(SwitchingAdControl);

            this.keywords = null;
            PlayAnimations = true;
            MaxFaultLevel = 10;

            if (DesignerProperties.IsInDesignTool)
                return;

            AdVisibilityService.AdsNoLongerShown += OnAdsNoLongerShown;
        }

        void OnAdsNoLongerShown(object sender, EventArgs e)
        {
            Dispose();
        }




        #region OnApplyTemplate

        public async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AdContainer = base.GetTemplateChild("AdContainer") as Panel;
            var fe = base.GetTemplateChild("LayoutRoot") as FrameworkElement;
            OnNewAdSB = fe.Resources["OnNewAdSB"] as Storyboard;

            if (DesignerProperties.IsInDesignTool)
            {
                var border = new Border { Background = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Colors.White), Width = 480, Height = 80, VerticalAlignment = VerticalAlignment.Top };
                var tb = new TextBlock { Text = "DESIGN-TIME TEXT", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                border.Child = tb;
                AdContainer.Children.Add(border);
                return;
            }

            if (AdVisibilityService.AreAdsStillBeingShownAtAll)
            {
                await Task.Delay(INITIAL_AD_DISPLAY_DELAY);
                CreateAd();
            }
            else
            {
                Dispose();
            }
        }

        #endregion




        async void CreateAd(bool advanceToNextProvider = false)
        {
            try
            {
                if (isDisposed)
                    return;

                adControl = await factory.CreateAdControl(keywords);

                if (this.isHidden)
                    adControl.Control.Visibility = Visibility.Collapsed;

                adControl.AdClicked += OnAdClicked;
                adControl.AdRefreshed += OnAdRefreshed;
                adControl.AdError += OnAdControlErrorOccurred;

                adControl.Control.Opacity = 0d;
                AdContainer.Children.Add(adControl.Control);
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
                Dispose();
            }
        }

        void FlushAd()
        {
            if (adControl == null)
                return;

            adControl.AdClicked -= OnAdClicked;
            adControl.AdRefreshed -= OnAdRefreshed;
            adControl.AdError -= OnAdControlErrorOccurred; 
            
            adControl.Dispose();

            if (adControl.Control != null && AdContainer.Children.Contains(adControl.Control))
            {
                AdContainer.Children.Remove(adControl.Control);
                adControl = null;
            }
        }




        #region Ad Control Event Handling

        void OnAdClicked(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                AdVisibilityService.AdEngaged();
            });
        }

        void OnAdRefreshed(object sender, EventArgs e)
        {
            adControl.Control.Opacity = 1d;
            currentFaultLevel = Math.Max(currentFaultLevel - 1, 0);

            if (OnNewAdSB == null || !PlayAnimations)
                return;

            OnNewAdSB.Stop();
            OnNewAdSB.Begin();
        }

        void OnAdControlErrorOccurred(object sender, EventArgs<Exception> e)
        {
            var error = string.Format("Ad Exception: {0}", e.Item);
            DebugEx.WriteLine(error);
            Dispatcher.BeginInvoke(() =>
            {
                FlushAd();
                currentFaultLevel++;
                if (currentFaultLevel < MaxFaultLevel)
                    CreateAd();
                else
                    Dispose();
            });
        }

        #endregion




        #region toggle AdControl visibility 
        
        public void ShowAdControl()
        {
            if (!isHidden)
                return;

            isHidden = false;
            if (adControl != null && adControl.Control != null)
                adControl.Control.Visibility = Visibility.Visible;
        }

        public void HideAdControl()
        {
            if (isHidden)
                return;

            isHidden = true;
            if (adControl != null && adControl.Control != null)
                adControl.Control.Visibility = Visibility.Collapsed;
        }

        #endregion




        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            try
            {
                this.Visibility = Visibility.Collapsed;
                FlushAd();
                AdVisibilityService.AdsNoLongerShown -= OnAdsNoLongerShown;
            }
            catch { }
        }
    }
}
