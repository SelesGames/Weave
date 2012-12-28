using Inneractive.Nokia.Ad;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SelesGames.UI.Advertising.Inneractive
{
    internal class InneractiveAdControlAdapter : IAdControlAdapter
    {
        UIElement adControlWrapper;

        public InneractiveAdControlAdapter(UIElement adControlWrapper)
        {
            this.adControlWrapper = adControlWrapper;
            var grid = new Grid();

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
            get { return adControlWrapper; }
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
