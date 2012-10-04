using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace weave
{
    public partial class ButtonBar : UserControl
    {
        public IObservable<Unit> RefreshButtonClicked { get; private set; }
        public IObservable<Unit> LocalSettingsButtonClicked { get; private set; }
        public IObservable<Unit> PreviousPageButtonClicked { get; private set; }
        public IObservable<Unit> NextPageButtonClicked { get; private set; }

        public ButtonBar()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            SetButtonColors();
            WireButtonEvents();
        }

        void SetButtonColors()
        {
            Brush foregroundBrush;
            //var osTheme = OSThemeHelper.GetCurrentTheme();
            //if (osTheme == OSTheme.Dark)
            //{
                foregroundBrush = AppSettings.Instance.Themes.CurrentTheme.AccentBrush;
            //}
            //else
            //{
            //    foregroundBrush = AppSettings.Palette.AccentDullBrush;
            //}

            this.refreshButton.Foreground = foregroundBrush;
            this.localSettingsButton.Foreground = foregroundBrush;
            this.previousPageButton.Foreground = foregroundBrush;
            this.nextPageButton.Foreground = foregroundBrush;
            this.previousPageButton.BorderBrush = foregroundBrush;
            this.nextPageButton.BorderBrush = foregroundBrush;
        }

        void WireButtonEvents()
        {
            RefreshButtonClicked = this.refreshButton.GetClick().Select(_ => Unit.Default);
            LocalSettingsButtonClicked = this.localSettingsButton.GetClick().Select(_ => Unit.Default);
            PreviousPageButtonClicked = this.previousPageButton.GetClick().Select(_ => Unit.Default);
            NextPageButtonClicked = this.nextPageButton.GetClick().Select(_ => Unit.Default);
        }
    }
}
