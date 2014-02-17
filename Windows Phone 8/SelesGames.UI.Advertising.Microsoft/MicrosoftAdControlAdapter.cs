using Microsoft.Advertising;
using Microsoft.Advertising.Mobile.UI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace SelesGames.UI.Advertising.Microsoft
{
    internal class MicrosoftAdControlAdapter : IAdControlAdapter
    {
        AdControl adControl;
        SerialDisposable sd = new SerialDisposable();

        public MicrosoftAdControlAdapter(AdControl adControl, int adRefreshTimeInSeconds)
        {
            if (adControl == null)
                throw new ArgumentNullException("adControl in MicrosoftAdControlAdapter.contructor");

            this.adControl = adControl;

            adControl.IsEngagedChanged += OnAdControlIsEngagedChanged;
            adControl.AdRefreshed += OnAdRefreshed;
            adControl.ErrorOccurred += OnAdControlErrorOccurred;

            sd.Disposable = Observable
                .Interval(TimeSpan.FromSeconds(adRefreshTimeInSeconds))
                .ObserveOnDispatcher()
                .Subscribe(
                    o =>
                    {
                        if (adControl != null && adControl.Visibility != Visibility.Collapsed)
                            adControl.Refresh();
                    },
                    exception => { ; });
        }




        #region event handlers

        void OnAdControlIsEngagedChanged(object sender, EventArgs e)
        {
            if (!adControl.IsEngaged)
                return;

            if (AdClicked != null)
                AdClicked(this, EventArgs.Empty);
        }

        void OnAdControlErrorOccurred(object sender, AdErrorEventArgs e)
        {
            if (AdError != null)
                AdError(this, new EventArgs<Exception> { Item = e.Error });
        }

        void OnAdRefreshed(object sender, EventArgs e)
        {
            if (AdRefreshed != null)
                AdRefreshed(this, EventArgs.Empty);
        }

        #endregion




        public UIElement Control
        {
            get { return adControl; }
        }

        public event EventHandler AdClicked;
        public event EventHandler AdRefreshed;
        public event EventHandler<EventArgs<Exception>> AdError;

        public void Dispose()
        {
            sd.Disposable = null;

            if (adControl == null)
                return;

            adControl.IsEngagedChanged -= OnAdControlIsEngagedChanged;
            adControl.ErrorOccurred -= OnAdControlErrorOccurred;
            adControl.AdRefreshed -= OnAdRefreshed;
        }
    }
}
