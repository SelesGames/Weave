using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Advertising;
using Microsoft.Advertising.Mobile.UI;

namespace weave.UI.Advertising
{
    public class TrialModeAdControl : Control, IDisposable
    {
        Panel AdContainer;
        Storyboard OnNewAdSB;
        AdControl adControl;
        AdUnitCollection adUnits;
        string keywords;
        bool isHidden = false;
        int currentFaultLevel = 0;
        bool isDisposed = false;
        SerialDisposable sd = new SerialDisposable();

        public bool PlayAnimations { get; set; }
        public bool IsAdCurrentlyEngaged { get { return false; } }
        public int MaxFaultLevel { get; set; }

        public TrialModeAdControl(string keywords)
            : this()
        {
            this.keywords = keywords;
        }

        public TrialModeAdControl()
        {
            DefaultStyleKey = typeof(TrialModeAdControl);

            this.keywords = null;
            PlayAnimations = true;
            MaxFaultLevel = 10;

            if (DesignerProperties.IsInDesignTool)
                return;

            this.adUnits = AdUnitCollection.Current;

            AdVisibilityService.AdsNoLongerShown += OnAdsNoLongerShown;
        }

        void OnAdsNoLongerShown(object sender, EventArgs e)
        {
            Dispose();
        }

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
                await TaskEx.Delay(TimeSpan.FromSeconds(1.5));
                CreateAd();
            }
            else
            {
                Dispose();
            }
        }

        async void CreateAd()
        {
            try
            {
                string appId;
                string adUnitId;

                await adUnits.AreAdUnitsSet;
                await TaskEx.Delay(TimeSpan.FromSeconds(1));
                if (isDisposed)
                    return;

                adUnitId = adUnits.GetRandomAdUnit();
                appId = AdSettings.AdApplicationId;

                if (string.IsNullOrEmpty(adUnitId) || string.IsNullOrEmpty(appId))
                {
                    Dispose();
                    return;
                }

                adControl = new AdControl(appId, adUnitId, false) { IsAutoCollapseEnabled = true, Width = 480, Height = 80, BorderBrush = new SolidColorBrush(Colors.White) };

                if (this.isHidden)
                    adControl.Visibility = Visibility.Collapsed;

                if (!string.IsNullOrEmpty(this.keywords))
                    adControl.Keywords = keywords;

                HookAdControlEvents();
                AdContainer.Children.Add(adControl);
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
                Dispose();
            }
        }

        void FlushAd()
        {
            UnhookAdControlEvents();

            if (AdContainer.Children.Contains(adControl))
            {
                AdContainer.Children.Remove(adControl);
                adControl = null;
            }
        }




        #region Ad Control Event Handling

        void HookAdControlEvents()
        {
            if (adControl == null)
                return;

            adControl.IsEngagedChanged += OnAdControlIsEngagedChanged;
            adControl.ErrorOccurred += OnAdControlErrorOccurred;
            adControl.AdRefreshed += OnAdRefreshed;

            sd.Disposable = Observable
                .Interval(TimeSpan.FromSeconds(30))
                .ObserveOnDispatcher()
                .Subscribe(
                    o =>
                    {
                        if (adControl != null && adControl.Visibility != Visibility.Collapsed)
                            adControl.Refresh();
                    },
                    exception => { ; });
        }

        void UnhookAdControlEvents()
        {
            sd.Disposable = null;

            if (adControl == null)
                return;

            adControl.IsEngagedChanged -= OnAdControlIsEngagedChanged;
            adControl.ErrorOccurred -= OnAdControlErrorOccurred;
            adControl.AdRefreshed -= OnAdRefreshed;
        }

        void OnAdControlIsEngagedChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (adControl.IsEngaged)
                {
                    AdVisibilityService.AdEngaged();
                }
            });
        }

        void OnAdControlErrorOccurred(object sender, AdErrorEventArgs e)
        {
            var error = string.Format("Ad ErrorCode: {0}, Exception: {1}", e.ErrorCode, e.Error);
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

        void OnAdRefreshed(object sender, EventArgs e)
        {
            currentFaultLevel = Math.Max(currentFaultLevel - 1, 0);

            if (OnNewAdSB == null || !PlayAnimations)
                return;

            OnNewAdSB.Stop();
            OnNewAdSB.Begin();
        }


        #endregion




        public void ShowAdControl()
        {
            if (!isHidden)
                return;

            isHidden = false;
            if (adControl != null)
                adControl.Visibility = Visibility.Visible;
        }

        public void HideAdControl()
        {
            if (isHidden)
                return;

            isHidden = true;
            if (adControl != null)
                adControl.Visibility = Visibility.Collapsed;
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            try
            {
                sd.Dispose();
                this.Visibility = Visibility.Collapsed;
                FlushAd();
                AdVisibilityService.AdsNoLongerShown -= OnAdsNoLongerShown;
            }
            catch { }
        }
    }
}
