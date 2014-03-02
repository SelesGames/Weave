using PocketSharp;
using SelesGames;
using System;
using System.Threading.Tasks;
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

        readonly static PocketHelper current = new PocketHelper();
        public static PocketHelper Current { get { return current; } }

        public PocketHelper()
        {
            this.consumerKey = AppSettings.Instance.ThirdParty.Pocket.ConsumerKey;
        }

        public async Task Save(NewsItem newsItem)
        {
            permState = ServiceResolver.Get<PermanentState>();
            var accessCode = permState.ThirdParty.PocketAccessCode;

            if (string.IsNullOrWhiteSpace(accessCode))
            {
                await Register();
            }

            else
            {
                await InnerSave(newsItem, async ex => await Register());
            }
        }

        async Task InnerSave(NewsItem newsItem, Action<Exception> onAddFail = null)
        {
            var client = new PocketClient(
                consumerKey: consumerKey,
                accessCode: permState.ThirdParty.PocketAccessCode,
                isMobileClient: true);

            try
            {
                var result = await client.Add(
                    uri: new Uri(newsItem.Link),
                    title: newsItem.Title,
                    tags: new[] { newsItem.Feed.Category, newsItem.Feed.Name });
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                if (onAddFail != null)
                    onAddFail(ex);
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

            GlobalNavigationService.ToOAuthPage(authenticationUri.OriginalString);

            permState.ThirdParty.PocketAccessCode = requestCode;
        }
    }
}
