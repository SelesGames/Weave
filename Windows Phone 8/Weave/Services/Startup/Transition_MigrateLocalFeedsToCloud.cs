using SelesGames;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Weave.Article.Service.Contracts;
using Weave.Article.Service.DTOs.ServerIncoming;
using Weave.SavedState;
using Weave.UI.Frame;
using Weave.ViewModels;


namespace Weave.Services.Startup
{
    public class Transition_MigrateLocalFeedsToCloud : IState
    {
        UserInfo user;
        PermanentState permState;
        OverlayFrame frame;
        List<Weave.Data.NewsItem> failedFavorites;

        public enum State
        {
            FeedsMigrated,
            Unnecessary,
            Fail
        }

        public State? CurrentState { get; private set; }

        public Transition_MigrateLocalFeedsToCloud(UserInfo user, PermanentState permState)
        {
            this.user = user;
            this.permState = permState;
            frame = ServiceResolver.Get<OverlayFrame>();
            failedFavorites = new List<Weave.Data.NewsItem>();
        }

        public async Task Transition()
        {
            if (permState.IsFirstTime)
            {
                CurrentState = State.Unnecessary;
                return;
            }

            var dal = new Weave.Data.Weave4DataAccessLayer();
            List<Weave.Data.FeedSource> existingFeeds = null;

            try
            {
                // migrate the feeds
                existingFeeds = await dal.Feeds.Value;
                user.Feeds = new ObservableCollection<Feed>(existingFeeds.Select(o =>
                    new Feed
                    {
                        Name = o.FeedName,
                        Uri = o.FeedUri,
                        ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
                        Category = o.Category,
                    }));
                await user.Create();
            }
            catch
            {
                CurrentState = State.Fail;
                MessageBox.Show("Unable to import your existing feeds.  Please ensure you have an internet connection, and relaunch the app to try again.", "Whoops!", MessageBoxButton.OK);
                Application.Current.Terminate();
            }

            // migrate any favorited articles
            var favorites = existingFeeds.SelectMany(o => o.News).Where(o => o.IsFavorite).ToList();

            var totalNumberOfFavorites = favorites.Count;
            int index = 0;

            foreach (var o in favorites)
            {
                frame.OverlayText = string.Format(
                    "Adding favorite articles ({0} remaining)", totalNumberOfFavorites - index++);
                await TrySaveFavoriteArticle(o);
            }

            CurrentState = State.FeedsMigrated;
        }




        #region Helper method to safely save the article

        async Task TrySaveFavoriteArticle(Weave.Data.NewsItem o)
        {
            try
            {
                await user.Bookmark(Convert(o), BookmarkType.Favorite);
                //await articleService.AddFavorite(
                //    user.Id,
                //    new SavedNewsItem
                //    {
                //        SourceName = o.OriginalSource,
                //        Link = o.Link,
                //        ImageUrl = o.ImageUrl,
                //        UtcPublishDateTime = o.PublishDateTime.ToString(),
                //        Title = o.Title,
                //        PodcastUri = o.PodcastUri,
                //        ZuneAppId = o.ZuneAppId,
                //        VideoUri = o.VideoUri,
                //        YoutubeId = o.YoutubeId,
                //    });
            }
            catch
            {
                failedFavorites.Add(o);
            }
        }

        NewsItem Convert(Data.NewsItem o)
        {
            return new NewsItem
            {
                Link = o.Link,
                Title = o.Title,
                ImageUrl = o.ImageUrl,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                VideoUri = o.VideoUri,
                YoutubeId = o.YoutubeId,
                UtcPublishDateTime = o.PublishDateTime.ToUniversalTime().ToString(),
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                IsFavorite = true,
                Feed = Convert(o.FeedSource),
            };
        }

        Feed Convert(Data.FeedSource o)
        {
            return new Feed
            {
                Id = o.Id,
                Name = o.FeedName,
                Uri = o.FeedUri,
                Category = o.Category,
            };
        }

        #endregion
    }
}
