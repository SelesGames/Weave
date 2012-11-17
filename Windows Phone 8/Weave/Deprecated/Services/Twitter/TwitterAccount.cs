using System;
using System.Runtime.Serialization;

namespace weave.Services.Twitter
{
    public static class TwitterAccount
    {
        const string ISO_KEY = "twitter_credentials";

        static bool hasAttemptedToGetCredentialsFromIsoStorage = false;
        static TwitterAccess credentials;
        static LazySingleton<TwitterAccess> credentialsHolder;

        static TwitterAccount()
        {
            credentialsHolder = new LazySingleton<TwitterAccess>(() =>
            {
                var serializer = new DataContractSerializer(typeof(TwitterAccess));
                return IsolatedStorageService.Get<TwitterAccess>(ISO_KEY,
                    stream => (TwitterAccess)serializer.ReadObject(stream));
            });
        }

        public static TwitterAccess CurrentTwitterAccessCredentials
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
                var serializer = new DataContractSerializer(typeof(TwitterAccess));
                IsolatedStorageService.Save<TwitterAccess>(credentials, ISO_KEY, (stream, o) => serializer.WriteObject(stream, o));
            }
        }
    }
}
