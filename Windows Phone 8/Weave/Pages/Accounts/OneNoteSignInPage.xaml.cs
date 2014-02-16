using Common.Microsoft;
using Microsoft.Live;
using Microsoft.Phone.Controls;
using SelesGames;
using SelesGames.Instapaper;
using System.Net;
using System.Net.Http;
using System.Windows;
using Weave.SavedState;
using Weave.Services.Instapaper;

namespace weave
{
    public partial class OneNoteSignInPage : PhoneApplicationPage
    {
        public OneNoteSignInPage()
        {
            InitializeComponent();
        }

        void OnSessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            var permState = ServiceResolver.Get<PermanentState>();

            switch (e.Status)
            {
                case LiveConnectSessionStatus.Connected:
                    permState.LiveAccessToken = new LiveOfflineAccessToken(
                        clientId: (string)Resources["ClientId"],
                        accessToken: e.Session.AccessToken,
                        accessTokenExpiration: e.Session.Expires,
                        refreshToken: e.Session.RefreshToken);

                    //SetCreateButtonsIsEnabled(true);
                    //infoTextBlock.Text = "Authentication successful";
                    NavigationService.GoBack();
                    break;
                case LiveConnectSessionStatus.NotConnected:
                    //SetCreateButtonsIsEnabled(false);
                    //infoTextBlock.Text = "Authentication failed.";
                    break;
                default:
                    //SetCreateButtonsIsEnabled(false);
                    //infoTextBlock.Text = "Not Authenticated";
                    break;
            }
        }
    }
}