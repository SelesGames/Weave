using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using Weave.Identity.Contracts;
using Weave.Identity.DTOs;
using Weave.ViewModels.Contracts.Client;

namespace weave.Pages.Accounts
{
    public partial class AccountSignInPage : PhoneApplicationPage
    {
        IIdentityService identityService;
        IUserCache userCache;

        public AccountSignInPage()
        {
            InitializeComponent();
        }

        async void OnFacebookButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileService = new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
                var mobileUser = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                var userId = mobileUser.UserId;
                var identityInfo = identityService.GetUserFromFacebookToken(userId);
            }
            catch (InvalidOperationException)
            {
                //message = "You must log in. Login Required";
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

        void OnLoginButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //try
            //{
            //    var mobileService = new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
            //    var mobileUser = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
            //    var userId = mobileUser.UserId;
            //}
            //catch (InvalidOperationException)
            //{
            //    //message = "You must log in. Login Required";
            //}
            //var accountClient = new Weave.Services.Account.Client();    
        }
    }
}