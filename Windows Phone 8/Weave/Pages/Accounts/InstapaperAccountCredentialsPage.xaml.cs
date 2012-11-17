using Microsoft.Phone.Controls;
using SelesGames.Instapaper;
using System.Net;
using System.Windows;
using weave.Services.Instapaper;

namespace weave
{
    public partial class InstapaperAccountCredentialsPage : PhoneApplicationPage
    {
        public InstapaperAccountCredentialsPage()
        {
            InitializeComponent();
        }

        async void saveButton_Click(object sender, System.EventArgs e)
        {
            var username = usernameField.Text;
            var password = passwordField.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Username is mandatory.");
                return;
            }

            var account = new InstapaperAccount 
            { 
                UserName = username, 
                Password = password 
            };
            string verifyUrl = account.CreateVerificationString();
            var request = HttpWebRequest.CreateHttp(verifyUrl);

            try
            {
                var temp = await request.GetResponseAsync();
                var response = temp as HttpWebResponse;
                if (response == null) return;

                if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Invalid username or password."));

                else if (response.StatusCode == HttpStatusCode.InternalServerError) // 500
                    Dispatcher.BeginInvoke(() => MessageBox.Show("There was an error contacting Instapaper.  Please try again in a few minutes."));

                else if (response.StatusCode == HttpStatusCode.OK) // 200
                {
                    InstapaperAccount2.CurrentInstapaperCredentials = account;
                    InstapaperService.FlushAnyPendingRequests();

                    Dispatcher.BeginInvoke(() =>
                    {
                        if (NavigationService.CanGoBack)
                            NavigationService.GoBack();
                    });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Whoops!  You either entered an invalid username/password combo, or Instapaper is down at the moment."));
            }
        }
    }
}