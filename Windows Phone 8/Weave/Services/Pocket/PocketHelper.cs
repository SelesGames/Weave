using PocketSharp;
using SelesGames;
using System;
using System.Threading.Tasks;
using System.Windows;
using Weave.SavedState;
using Weave.Settings;
using Weave.ViewModels;

namespace Weave.Services.Pocket
{
    public class PocketHelper
    {
        const string CALLBACK_URI = "http://www.selesgames.com";

        readonly string consumerKey;

        PermanentState permState;
        NewsItem currentNewsItem;

        readonly static PocketHelper current = new PocketHelper();
        public static PocketHelper Current { get { return current; } }

        public PocketHelper()
        {
            this.consumerKey = AppSettings.Instance.ThirdParty.Pocket.ConsumerKey;
        }

        public async Task Save(NewsItem newsItem)
        {
            if (newsItem == null)
                return;

            currentNewsItem = newsItem;

            try
            {
                await InnerSave(newsItem)
                    .ContinueWith(async _ => await Register(), TaskContinuationOptions.OnlyOnFaulted);
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Unable to save to Pocket");
            }
        }

        async Task InnerSave(NewsItem newsItem)
        {
            permState = ServiceResolver.Get<PermanentState>();
            var accessCode = permState.ThirdParty.PocketAccessCode;

            if (string.IsNullOrWhiteSpace(accessCode))
            {
                throw new Exception("access code not set");
            }

            var client = new PocketClient(
                consumerKey: consumerKey,
                accessCode: accessCode,
                isMobileClient: true);

            try
            {
                ToastService.ToastPrompt("Sending to Pocket...");

                var result = await client.Add(
                    uri: new Uri(newsItem.Link),
                    title: newsItem.Title,
                    tags: new[] { newsItem.Feed.Category, newsItem.Feed.Name });

                ToastService.ToastPrompt("Saved to Pocket!");
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                throw;
            }
        }

        async Task Register()
        {
            var client = new PocketClient(
                consumerKey: consumerKey,
                callbackUri: CALLBACK_URI,
                isMobileClient: true);

            var requestCode = await client.GetRequestCode();
            var authenticationUri = client.GenerateAuthenticationUri(requestCode);

            await GlobalDispatcher.Current.InvokeAsync(() =>
                GlobalNavigationService.ToOAuthPage(
                    authenticationUri.OriginalString,
                    async () => await OnCallback(requestCode)));
        }

        async Task OnCallback(string requestCode)
        {
            bool isRegSuccessful = false;

            var client = new PocketClient(consumerKey: consumerKey);

            try
            {
                var response = await client.GetUser(requestCode: requestCode);
                permState.ThirdParty.PocketAccessCode = response.Code;
                isRegSuccessful = true;
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
            }

            if (isRegSuccessful && currentNewsItem != null)
                await InnerSave(currentNewsItem);
        }
    }
}
