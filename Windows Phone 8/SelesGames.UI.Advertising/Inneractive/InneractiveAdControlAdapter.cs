using Inneractive.Nokia.Ad;
using System;
using System.Windows;

namespace SelesGames.UI.Advertising.Inneractive
{
    public class InneractiveAdControlAdapter : IAdControlAdapter
    {
        InneractiveAd adControl;

        public InneractiveAdControlAdapter(InneractiveAd adControl)
        {
            this.adControl = adControl;

            InneractiveAd.AdClicked += OnAdClicked;
            InneractiveAd.AdReceived += OnAdRefreshed;
            InneractiveAd.DefaultAdReceived += OnAdRefreshed;
            InneractiveAd.AdFailed += OnAdFailed;  
        }




        #region event handlers

        void OnAdClicked(object sender)
        {
            if (AdClicked != null)
                AdClicked(this, EventArgs.Empty);
        }

        void OnAdRefreshed(object sender)
        {
            if (AdRefreshed != null)
                AdRefreshed(this, EventArgs.Empty);
        }

        void OnAdFailed(object sender)
        {
            if (AdError != null)
                AdError(this, new EventArgs<Exception> { Item = new Exception("Inneractive ad failed") });
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
            InneractiveAd.AdClicked -= OnAdClicked;
            InneractiveAd.AdReceived -= OnAdRefreshed;
            InneractiveAd.DefaultAdReceived -= OnAdRefreshed;
            InneractiveAd.AdFailed -= OnAdFailed;  
        }
    }
}
