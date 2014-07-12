using MillennialMedia.WP7.SDK;
using System;
using System.Windows;

namespace SelesGames.UI.Advertising.MillennialMedia
{
    internal class MillennialMediaAdControlAdapter : IAdControlAdapter
    {
        MMBannerAdView adView;

        public MillennialMediaAdControlAdapter(MMBannerAdView adView)
        {
            this.adView = adView;

            MMSDK.MMAdEvents += MMSDK_MMAdEvents;
        }

        void MMSDK_MMAdEvents(object sender, MMEventArgs e)
        {
            adView.Dispatcher.BeginInvoke(() =>
            {
                if (e.Exception != null)
                {
                    if (AdError != null)
                        AdError(this, new EventArgs<Exception> { Item = e.Exception });
                }

                switch (e.Type)
                {
                    case MMEventType.GetAdSuccess:
                        if (AdRefreshed != null)
                            AdRefreshed(this, EventArgs.Empty);
                        break;

                    case MMEventType.GetAdFailure:
                        if (AdRefreshed != null)
                            AdRefreshed(this, EventArgs.Empty);
                        break;
                }
            });
        }




        public UIElement Control
        {
            get { return adView; }
        }

        public event EventHandler AdClicked;
        public event EventHandler AdRefreshed;
        public event EventHandler<EventArgs<Exception>> AdError;

        public void Dispose()
        {
            MMSDK.MMAdEvents -= MMSDK_MMAdEvents;
        }
    }
}
