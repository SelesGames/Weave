using AdDuplex;
using System;
using System.Windows;

namespace SelesGames.UI.Advertising.AdDuplex
{
    internal class AdDuplexAdControlAdapter : IAdControlAdapter
    {
        AdControl adControl;

        public AdDuplexAdControlAdapter(AdControl adControl)
        {
            this.adControl = adControl;

            adControl.AdClick += OnAdClicked;
            adControl.AdLoaded += OnAdRefreshed;
            adControl.AdLoadingError += OnAdFailed;
        }




        #region event handlers

        void OnAdClicked(object sender, AdClickEventArgs e)
        {
            if (AdClicked != null)
                AdClicked(this, EventArgs.Empty);
        }

        void OnAdRefreshed(object sender, AdLoadedEventArgs e)
        {
            if (AdRefreshed != null)
                AdRefreshed(this, EventArgs.Empty);
        }

        void OnAdFailed(object sender, AdLoadingErrorEventArgs e)
        {       
            if (AdError != null)
                AdError(this, new EventArgs<Exception> { Item = e.Error });
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
            adControl.AdClick -= OnAdClicked;
            adControl.AdLoaded -= OnAdRefreshed;
            adControl.AdLoadingError -= OnAdFailed;
        }
    }
}
