using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SelesGames.Instapaper
{
    public static class InstapaperAccountExtensions
    {
        public static async Task<InstapaperResult> SendToInstapaper(this InstapaperAccount account, string link, string title, string description = null)
        {
            if (account == null)
                return new InstapaperResult { ResultType = InstapaperResultType.CredentialsMissing };

            try
            {
                var url = account.CreateAddToInstapaperUrlString(link, title, description);

                var response = await new HttpClient().GetAsync(url).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                    return new InstapaperResult { ResultType = InstapaperResultType.InvalidCredentials };

                else if (response.StatusCode == HttpStatusCode.BadRequest) // 400
                    return new InstapaperResult { ResultType = InstapaperResultType.BadRequest };

                else if (response.StatusCode == HttpStatusCode.InternalServerError) // 500
                    return new InstapaperResult { ResultType = InstapaperResultType.ErrorContactingInstapaper };

                else if (response.StatusCode == HttpStatusCode.Created) // 201
                    return new InstapaperResult { ResultType = InstapaperResultType.Created };

                throw new Exception("Unexpected result");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
