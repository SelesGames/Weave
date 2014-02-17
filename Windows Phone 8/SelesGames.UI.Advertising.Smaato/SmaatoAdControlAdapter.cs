using SOMAWP8;
using System;
using System.Windows;

namespace SelesGames.UI.Advertising.Smaato
{
    internal class SmaatoAdControlAdapter : IAdControlAdapter
    {
        SomaAdViewer adViewer;

        public SmaatoAdControlAdapter(SomaAdViewer adViewer)
        {
            this.adViewer = adViewer;

            adViewer.AdClick += OnAdClicked;
            adViewer.NewAdAvailable += OnAdRefreshed;
            adViewer.AdError += OnAdFailed;
        }




        #region event handlers

        void OnAdClicked(object sender, EventArgs e)
        {
            if (AdClicked != null)
                AdClicked(this, EventArgs.Empty);
        }

        void OnAdRefreshed(object sender, EventArgs e)
        {
            if (AdRefreshed != null)
                AdRefreshed(this, EventArgs.Empty);
        }

        void OnAdFailed(object sender, string ErrorCode, string ErrorDescription)
        {
            var message = string.Format("Smaato ad error:\r\nErrorCode: {0},\r\nErrorDescription: {1}", ErrorCode, ErrorDescription);
            
            if (AdError != null)
                AdError(this, new EventArgs<Exception> { Item = new Exception(message) });
        }

        #endregion




        public UIElement Control
        {
            get { return adViewer; }
        }

        public event EventHandler AdClicked;
        public event EventHandler AdRefreshed;
        public event EventHandler<EventArgs<Exception>> AdError;

        public void Dispose()
        {
            adViewer.AdClick -= OnAdClicked;
            adViewer.NewAdAvailable -= OnAdRefreshed;
            adViewer.AdError -= OnAdFailed;

            adViewer.StopAds();
        }
    }
}
