using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;
using SelesGames;
using System;
using System.Net;
using System.Windows;
using Weave.Identity.Service.Client;
using Weave.Identity.Service.Contracts;
using Weave.Identity.Service.DTOs;
using Weave.ViewModels.Contracts.Client;

namespace weave.Pages.Accounts
{
    public partial class AccountSignInPage : PhoneApplicationPage
    {
        IIdentityService identityService = new ServiceClient();
        IUserCache userCache;

        public AccountSignInPage()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            userCache = ServiceResolver.Get<IUserCache>();
        }

        async void OnFacebookButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            bool error = false;
            IdentityInfo identityInfo;
            string identityUserId = null;

            try
            {
                var mobileService = new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
                var mobileUser = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                identityUserId = mobileUser.UserId;
                identityInfo = await identityService.GetUserFromFacebookToken(identityUserId);
            }
            catch (WebException webException)
            {
                var response = (HttpWebResponse)webException.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                    error = true;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }

            // if error, create new identityInfo, then upload
            if (error)
            {
                var user = userCache.Get();
                identityInfo = new IdentityInfo
                {
                    UserId = user.Id,
                    FacebookAuthToken = identityUserId,
                };
                await identityService.Add(identityInfo);
            }
            else
            {
                // replace userId of cached user with the userId returned from identityInfo
            }
        }

        async void OnTwitterButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileService = new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
                var mobileUser = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.Twitter);
                var userId = mobileUser.UserId;
                var identityInfo = await identityService.GetUserFromTwitterToken(userId);

                bool error = false;

                // if error, create new identityInfo, then upload
                if (error)
                {
                    var user = userCache.Get();
                    identityInfo = new IdentityInfo
                    {
                        UserId = user.Id,
                        TwitterAuthToken = userId,
                    };
                    await identityService.Add(identityInfo);
                }
                else
                {
                    // replace userId of cached user with the userId returned from identityInfo
                }
            }
            catch (InvalidOperationException)
            {
                //message = "You must log in. Login Required";
            }
        }

        async void OnGoogleButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileService = new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
                var mobileUser = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.Google);
                var userId = mobileUser.UserId;
                var identityInfo = await identityService.GetUserFromGoogleToken(userId);

                bool error = false;

                // if error, create new identityInfo, then upload
                if (error)
                {
                    var user = userCache.Get();
                    identityInfo = new IdentityInfo
                    {
                        UserId = user.Id,
                        GoogleAuthToken = userId,
                    };
                    await identityService.Add(identityInfo);
                }
                else
                {
                    // replace userId of cached user with the userId returned from identityInfo
                }
            }
            catch (InvalidOperationException)
            {
                //message = "You must log in. Login Required";
            }
        }

        async void OnLoginButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var username = userNameTB.Text;
                var password = passwordTB.Text;

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("You must enter a username to sign-in");
                    return;
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("You must enter a password to sign-in");
                    return;
                } 
                
                var identityInfo = await identityService.GetUserFromUserNameAndPassword(username, password);

                bool error = false;

                // if error, create new identityInfo, then upload
                if (error)
                {
                    // alert user that username/password didn't work
                }
                else
                {
                    // replace userId of cached user with the userId returned from identityInfo
                }
            }
            catch (InvalidOperationException)
            {
                //message = "You must log in. Login Required";
            }  
        }

        void OnCreateAccountButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GlobalNavigationService.ToCreateAccountPage();  
        }
    }
}