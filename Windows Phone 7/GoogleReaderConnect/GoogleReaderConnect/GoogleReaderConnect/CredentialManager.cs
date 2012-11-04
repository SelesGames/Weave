using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GoogleReaderConnect
{
    public static class CredentialManager
    {
        #region Credential Manager Methods

        /// <summary>
        /// Stores a users credentials to Google Reader in the form "username;password". 
        /// This is only called once the credentials have been verified.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public static void StoreCredentials(string username, string password)
        {
            string credentials = String.Format("{0};{1}", username, password);
            StreamReader reader;
            StreamWriter writer;

            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // This is the first time storing credentials, so create the file to store
                    if (!isoStore.FileExists(@"GReader\UserCredentials.txt"))
                    {
                        isoStore.CreateDirectory(@"GReader");
                        isoStore.CreateFile(@"GReader\UserCredentials.txt");
                    }

                    reader = new StreamReader(new IsolatedStorageFileStream(@"GReader\UserCredentials.txt",
                                                               FileMode.Open, isoStore));

                    // This is what is already in Isolated Storage
                    var storedCredentials = reader.ReadLine();

                    reader.Close();

                    // The case not handled here is if the stored credentials are equivalent to the credentials
                    // In that case there is no need to write over, so ignore
                    // There was nothing in isolated storage previously, store the credentials for the first time
                    if (String.IsNullOrEmpty(storedCredentials))
                    {
                        writer = new StreamWriter(new IsolatedStorageFileStream(@"GReader\UserCredentials.txt",
                                                           FileMode.Open, isoStore));
                        writer.WriteLine(credentials);
                        writer.Close();
                    }
                    // The username and/or password have changed since the last login, so overwrite
                    else if (storedCredentials != credentials)
                    {
                        writer = new StreamWriter(new IsolatedStorageFileStream(@"GReader\UserCredentials.txt",
                                                           FileMode.Truncate, isoStore));
                        writer.WriteLine(credentials);
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return;
        }

        /// <summary>
        /// Gets the credentials of a Google Reader user in the form "username;password"
        /// </summary>
        /// <returns>A user's credentials as "username;password" or empty string
        /// if there are no stored credentials</returns>
        public static string GetCredentials()
        {
            string credentials = string.Empty;

            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // This user has credentials
                    if (isoStore.FileExists(@"GReader\UserCredentials.txt"))
                    {
                        StreamReader reader = new StreamReader(new IsolatedStorageFileStream(@"GReader\UserCredentials.txt",
                                                               FileMode.Open, isoStore));

                        // This is how the credentials are stored
                        credentials = reader.ReadLine();

                        reader.Close();
                    }
                    // The credentials haven't been stored for this user
                    else
                    {
                        credentials = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return credentials;
        }

        #endregion // Credential Manager Methods
    }
}
