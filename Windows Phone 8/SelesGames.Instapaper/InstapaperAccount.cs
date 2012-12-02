
namespace SelesGames.Instapaper
{
    public class InstapaperAccount
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public bool CanAdd { get { return !string.IsNullOrEmpty(UserName); } }

        public string CreateAddToInstapaperUrlString(string url, string title = null, string selection = null)
        {
            var addRequest = InstapaperAddRequest.CreateRequest(UserName, url);
            if (!string.IsNullOrEmpty(Password))
                addRequest.WithPassword(Password);

            if (!string.IsNullOrEmpty(title))
                addRequest.WithTitle(title);

            if (!string.IsNullOrEmpty(selection))
                addRequest.WithSelection(selection);

            return addRequest.GetUri();
        }

        public string CreateVerificationString()
        {
            return InstapaperVerifyRequest.GetUri(UserName, Password);
        }
    }
}
