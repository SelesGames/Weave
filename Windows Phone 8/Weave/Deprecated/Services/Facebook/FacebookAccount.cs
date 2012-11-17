using System;
using System.Runtime.Serialization;

namespace weave.Services.Facebook
{
    public static class FacebookAccount
    {
        const string ISO_KEY = "facebook_credentials";

        static bool hasAttemptedToGetCredentialsFromIsoStorage = false;
        static FacebookAccess credentials;
        static LazySingleton<FacebookAccess> credentialsHolder;

        static FacebookAccount()
        {
            credentialsHolder = new LazySingleton<FacebookAccess>(() =>
            {
                var serializer = new DataContractSerializer(typeof(FacebookAccess));
                return IsolatedStorageService.Get<FacebookAccess>(ISO_KEY,
                    stream => (FacebookAccess)serializer.ReadObject(stream));
            });
        }

        public static FacebookAccess CurrentfacebookAccessCredentials
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
                var serializer = new DataContractSerializer(typeof(FacebookAccess));
                IsolatedStorageService.Save<FacebookAccess>(credentials, ISO_KEY, (stream, o) => serializer.WriteObject(stream, o));
            }
        }

        public static void Save()
        {
            var serializer = new DataContractSerializer(typeof(FacebookAccess));
            IsolatedStorageService.Save<FacebookAccess>(credentials, ISO_KEY, (stream, o) => serializer.WriteObject(stream, o));
        }
    }
}
