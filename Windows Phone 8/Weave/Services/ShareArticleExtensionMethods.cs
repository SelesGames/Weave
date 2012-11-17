using weave.Services.Instapaper;

namespace weave
{
    public static class ShareArticleExtensionMethods
    {
        public static void ShareToEmail(this INewsItem newsItem)
        {
            if (newsItem == null)
                return;

            SelesGames.Phone.TaskService.ToEmailComposeTask(
                Subject: newsItem.Title,
                Body: string.Format("{0} [shared from {3} for Windows Phone]\r\n\r\n{1}",
                    newsItem.Title,
                    newsItem.Link,
                    string.Empty,
                    AppSettings.Instance.AppName));
        }

        public static void ShareToSocial(this INewsItem newsItem)
        {
            if (newsItem == null)
                return;

            SelesGames.Phone.TaskService.ToShareLinkTask(newsItem.Link, newsItem.Title, newsItem.Title);
        }

        public static void ShareToSms(this INewsItem newsItem)
        {
            if (newsItem == null)
                return;

            SelesGames.Phone.TaskService.ToSmsComposeTask(string.Format("{0} {1}", newsItem.Title, newsItem.Link));
        }

        public static void SendToInstapaper(this INewsItem newsItem)
        {
            if (newsItem == null)
                return;

            InstapaperService.SendToInstapaper(newsItem);
        }

        public static void SendToInternetExplorer(this INewsItem newsItem)
        {
            if (newsItem == null)
                return;

            if (newsItem is NewsItem)
                ((NewsItem)newsItem).HasBeenViewed = true;

            GlobalNavigationService.ToInternetExplorer(newsItem);
        }
    }
}
