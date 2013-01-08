using MobFox.Ads;
using System;
using System.Windows;

namespace SelesGames.UI.Advertising.MobFox
{
    internal class MobFoxAdControlAdapter : IAdControlAdapter
    {
        AdControl adControl;

        public MobFoxAdControlAdapter(AdControl adControl)
        {
            this.adControl = adControl;

            adControl.AdEngaged += OnAdClicked;
            adControl.NewAd += OnAdRefreshed;
            adControl.NoAd += OnAdFailed;
        }




        #region event handlers

        void OnAdClicked(object sender, AdEngagedEventArgs e)
        {
            if (AdClicked != null)
                AdClicked(this, EventArgs.Empty);
        }

        void OnAdRefreshed(object sender, NewAdEventArgs e)
        {
            if (AdRefreshed != null)
                AdRefreshed(this, EventArgs.Empty);
        }

        void OnAdFailed(object sender, NoAdEventArgs args)
        {
            var message = string.Format("No ads for MobFox");
            
            if (AdError != null)
                AdError(this, new EventArgs<Exception> { Item = new Exception(message) });
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
            adControl.AdEngaged -= OnAdClicked;
            adControl.NewAd -= OnAdRefreshed;
            adControl.NoAd -= OnAdFailed;
        }
    }
}
