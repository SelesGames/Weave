using System;
using SelesGames.Instapaper;
using SelesGames.WP.IsoStorage;

namespace weave.Services.Instapaper
{
    public static class InstapaperAccount2
    {
        const string ISO_KEY = "instapaper_credentials";

        static bool hasAttemptedToGetCredentialsFromIsoStorage = false;
        static InstapaperAccount credentials;
        static Lazy<InstapaperAccount> credentialsHolder;

        static InstapaperAccount2()
        {
            credentialsHolder = new Lazy<InstapaperAccount>(() =>
            {
                var task = new DataContractIsoStorageClient<InstapaperAccount>().GetAsync(ISO_KEY, System.Threading.CancellationToken.None);
                task.Wait();
                return task.Result;
            });
        }

        public static InstapaperAccount CurrentInstapaperCredentials
        {
            get
            {
                if (!hasAttemptedToGetCredentialsFromIsoStorage)
                {
                    credentials = credentialsHolder.Get();
                    hasAttemptedToGetCredentialsFromIsoStorage = true;
                }
                return credentials;
            }
            set
            {
                credentials = value;
                new DataContractIsoStorageClient<InstapaperAccount>().SaveAsync(ISO_KEY, credentials, System.Threading.CancellationToken.None);
            }
        }
    }
}
