using Inneractive.Nokia.Ad;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelesGames.UI.Advertising.Inneractive
{
    internal class InneractiveAdControlAdapter : IAdControlAdapter
    {
        InneractiveAd adControl;
        Border adControlWrapper;

        public InneractiveAdControlAdapter(InneractiveAd adControl)
        {
            this.adControl = adControl;

            adControlWrapper = new Border
            {
                BorderBrush = new SolidColorBrush { Color = Colors.White },
                Background = Application.Current.Resources["PhoneChromeBrush"] as Brush,
                BorderThickness = new Thickness(1),
            };
            adControlWrapper.Child = adControl;


            adControl.AdClicked += OnAdClicked;
            adControl.AdReceived += OnAdRefreshed;
            adControl.DefaultAdReceived += OnAdRefreshed;
            adControl.AdFailed += OnAdFailed;  
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
            adControl.AdClicked -= OnAdClicked;
            adControl.AdReceived -= OnAdRefreshed;
            adControl.DefaultAdReceived -= OnAdRefreshed;
            adControl.AdFailed -= OnAdFailed;  
        }
    }
}
