using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using Weave.GoogleReader;

namespace GoogleReaderConnect
{
    /// <summary>
    /// Outlines the UI handling to pull down a users list of feeds
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        GoogleReader gr;

        // Constructor
        public MainPage()
        {
            CredentialObject credentials;       

            InitializeComponent();

            SupportedOrientations = SupportedPageOrientation.Portrait | SupportedPageOrientation.Landscape;

            credentials = IsolatedStorageService.GetGoogleReaderCredentials();

            if (credentials != null)
            {
                textBoxUsername.Text = credentials.Username;
                textBoxPassword.Password = credentials.Password;
            }
        }

        private async void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxUsername.Text) && !String.IsNullOrEmpty(textBoxPassword.Password))
            {
                try
                {
                    gr = new GoogleReader(textBoxUsername.Text, textBoxPassword.Password);//, "GoogleReaderConnect");
                    await gr.Authenticate();
                    AfterAuthCall();
                    await gr.LoadSubscriptionList();
                    Debug.WriteLine(gr.Feeds);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show("Please enter both a username and password.", "Missing Credentials", MessageBoxButton.OK);
            }

        }

        void AfterAuthCall()
        {
            if (gr.AuthResult == GoogleReader.AuthenticationResult.OK)
            {
                MessageBox.Show("Successfully Connected! Pulling down feeds...");

                // Store the credentials
                CredentialObject credentials = new CredentialObject();
                credentials.Username = textBoxUsername.Text;
                credentials.Password = textBoxPassword.Password;

                IsolatedStorageService.SaveCredentials(credentials);
            }
            //else
            //{
            //    MessageBox.Show(gr.AuthToken, "Error", MessageBoxButton.OK);
            //}

        }
    }
}