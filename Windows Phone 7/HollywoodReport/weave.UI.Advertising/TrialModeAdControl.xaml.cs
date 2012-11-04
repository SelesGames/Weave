using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Advertising.Mobile.UI;

namespace weave.UI.Advertising
{
    public partial class TrialModeAdControl : UserControl, IDisposable
    {
        AdControl adControl;
        CompositeDisposable disposables = new CompositeDisposable();
        string keywords;
        bool isHidden = false;

        public bool PlayAnimations { get; set; }
        public bool IsAdCurrentlyEngaged { get; set; }

        static TrialModeAdControl()
        {
            AdControl.TestMode = false;
        }

        public TrialModeAdControl()
        {
            this.keywords = null;
            InitializeComponent();
            PlayAnimations = true;

            if (this.IsInDesignMode())
                return;

            if (AdVisibilityService.AreAdsStillBeingShownAtAll)
            {
                InitializeAdControl();
            }
            else
            {
                DestroyAndRemoveAd();
            }

            AdVisibilityService
                .AdsNoLongerShown
                .Take(1)
                .Subscribe(o => DestroyAndRemoveAd())
                .DisposeWith(disposables);
        }

        public TrialModeAdControl(string keywords) : this()
        {
            this.keywords = keywords;
            //InitializeComponent();
            //PlayAnimations = true;

            //if (this.IsInDesignMode())
            //    return;

            //if (AdVisibilityService.AreAdsStillBeingShownAtAll)
            //{
            //    InitializeAdControl();
            //}
            //else
            //{
            //    DestroyAndRemoveAd();
            //}

            //AdVisibilityService
            //    .AdsNoLongerShown
            //    .Take(1)
            //    .Subscribe(o => DestroyAndRemoveAd())
            //    .DisposeWith(disposables);
        }

        public void ShowAdControl()
        {
            isHidden = false;
            if (adControl != null)
                adControl.Visibility = Visibility.Visible;
        }

        public void HideAdControl()
        {
            isHidden = true;
            if (adControl != null)
                adControl.Visibility = Visibility.Collapsed;
        }

        void InitializeAdControl()
        {
            AdSettings.AdUnits
                .AdUnitsSet()
                .Take(1)
                .ObserveOnDispatcher()
                .Subscribe(FinishInitialization, HandleAdUnitsSetException)
                .DisposeWith(disposables);
        }

        void FinishInitialization()
        {
            var appId = AdSettings.AdApplicationId;
            var adUnitId = AdSettings.AdUnits.GetRandomAdUnit();

            if (string.IsNullOrEmpty(adUnitId) || string.IsNullOrEmpty(appId))
            {
                DestroyAndRemoveAd();
                return;
            }

            adControl = new AdControl(appId, adUnitId, AdModel.Contextual, false);

            if (this.isHidden)
                adControl.Visibility = Visibility.Collapsed;

            LayoutRoot.Children.Add(adControl);

            Binding b = new Binding("AdHeight") { Source = this };
            adControl.SetBinding(AdControl.HeightProperty, b);

            b = new Binding("AdWidth") { Source = this };
            adControl.SetBinding(AdControl.WidthProperty, b);

            b = new Binding("AdMargin") { Source = this };
            adControl.SetBinding(AdControl.MarginProperty, b);


            adControl.AdEngaged += (s, e) =>
            {
                IsAdCurrentlyEngaged = true;
                AdVisibilityService.AdEngaged();
            };
            adControl.AdDisengaged += (s, e) => IsAdCurrentlyEngaged = false;

            Observable.FromEventPattern<ErrorEventArgs>(adControl, "AdControlError")
                .Take(1)
                .ObserveOnDispatcher()
                .Subscribe(notUsed => DestroyAndRemoveAd())
                .DisposeWith(disposables);


            Observable
                .Interval(TimeSpan.FromSeconds(30))
                .ObserveOnDispatcher()
                .Subscribe(
                    o =>
                    {
                        if (adControl != null && adControl.Visibility != Visibility.Collapsed)
                        {
                            adControl.RequestNextAd();
                            if (PlayAnimations)
                                RubberBandSB.Begin();
                        }
                    },
                    exception => { ; })
                .DisposeWith(disposables);

            //AdVisibilityService.GetLocationAsync().Subscribe(
            //    location => adControl.Location = location,
            //    exception => { ; });
        }

        void HandleAdUnitsSetException(Exception exception)
        {
            DestroyAndRemoveAd();
        }

        void DestroyAndRemoveAd()
        {
            try
            {
                LayoutRoot.Visibility = Visibility.Collapsed;

                if (adControl == null)
                    return;

                if (LayoutRoot.Children.Contains(adControl))
                {
                    LayoutRoot.Children.Remove(adControl);
                    adControl = null;
                }
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            disposables.Dispose();
            try
            {
                if (adControl == null)
                    return;

                if (LayoutRoot.Children.Contains(adControl))
                {
                    LayoutRoot.Children.Remove(adControl);
                    adControl = null;
                }
            }
            catch (Exception) { }
        }




        #region Dependency Properties

        public static readonly DependencyProperty AdWidthProperty = DependencyProperty.Register(
            "AdWidth",
            typeof(double),
            typeof(TrialModeAdControl),
            new PropertyMetadata(480d, new PropertyChangedCallback(OnAdWidthChanged)));

        public double AdWidth
        {
            get { return (double)GetValue(AdWidthProperty); }
            set { SetValue(AdWidthProperty, value); }
        }

        static void OnAdWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TrialModeAdControl tmac = (TrialModeAdControl)d;
            var newWidth = (double)e.NewValue;
            tmac.LayoutRoot.Width = newWidth + tmac.AdMargin.Left + tmac.AdMargin.Right;
        }

        public static readonly DependencyProperty AdHeightProperty = DependencyProperty.Register(
            "AdHeight",
            typeof(double),
            typeof(TrialModeAdControl),
            new PropertyMetadata(80d, new PropertyChangedCallback(OnAdHeightChanged)));

        public double AdHeight
        {
            get { return (double)GetValue(AdHeightProperty); }
            set { SetValue(AdHeightProperty, value); }
        }

        static void OnAdHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TrialModeAdControl tmac = (TrialModeAdControl)d;
            var newHeight = (double)e.NewValue;
            tmac.LayoutRoot.Height = newHeight + tmac.AdMargin.Top + tmac.AdMargin.Bottom;
        }

        public static readonly DependencyProperty AdMarginProperty = DependencyProperty.Register(
            "AdMargin",
            typeof(Thickness),
            typeof(TrialModeAdControl),
            new PropertyMetadata(new Thickness(0), new PropertyChangedCallback(OnAdMarginChanged)));

        public Thickness AdMargin
        {
            get { return (Thickness)GetValue(AdMarginProperty); }
            set { SetValue(AdMarginProperty, value); }
        }

        static void OnAdMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TrialModeAdControl tmac = (TrialModeAdControl)d;
            var newMargin = (Thickness)e.NewValue;
            tmac.LayoutRoot.Width = tmac.AdWidth + newMargin.Left + newMargin.Right;
            tmac.LayoutRoot.Height = tmac.AdHeight + newMargin.Top + newMargin.Bottom;
        }

        #endregion
    }
}
