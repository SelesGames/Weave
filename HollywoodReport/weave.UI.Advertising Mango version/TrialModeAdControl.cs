using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Advertising;
using Microsoft.Advertising.Mobile.UI;

namespace weave.UI.Advertising
{
    public class TrialModeAdControl : Control, IDisposable
    {
        Panel AdContainer;
        AdControl adControl;
        AdUnitCollection adUnits;
        string keywords;
        bool isHidden = false;
        int currentFaultLevel = 0;
        bool isDisposed = false;

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
            MaxFaultLevel = 5;

            if (DesignerProperties.IsInDesignTool)
                return;

            this.adUnits = AdUnitCollection.Current;

            AdVisibilityService.AdsNoLongerShown += OnAdsNoLongerShown;
        }

        void OnAdsNoLongerShown(object sender, EventArgs e)
        {
            Dispose();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AdContainer = base.GetTemplateChild("AdContainer") as Panel;

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
                if (isDisposed)
                    return;

                adUnitId = adUnits.GetRandomAdUnit();
                appId = AdSettings.AdApplicationId;

                if (string.IsNullOrEmpty(adUnitId) || string.IsNullOrEmpty(appId))
                {
                    Dispose();
                    return;
                }

                adControl = new AdControl(appId, adUnitId, true) { IsAutoCollapseEnabled = true, Width = 480, Height = 80, BorderBrush = new SolidColorBrush(Colors.White) };

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
        }

        void UnhookAdControlEvents()
        {
            if (adControl == null)
                return;

            adControl.IsEngagedChanged -= OnAdControlIsEngagedChanged;
            adControl.ErrorOccurred -= OnAdControlErrorOccurred;
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
                this.Visibility = Visibility.Collapsed;
                FlushAd();
                AdVisibilityService.AdsNoLongerShown -= OnAdsNoLongerShown;
            }
            catch { }
        }
    }
}
