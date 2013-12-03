using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Weave.Article.Service.Contracts;
using Weave.SavedState;
using Weave.ViewModels;
using Weave.Article.Service.DTOs.ServerIncoming;


namespace Weave.Services.Startup
{
    public class Transition_MigrateLocalFeedsToCloud : IState
    {
        UserInfo user;
        PermanentState permState;
        IWeaveArticleService articleService;

        public Transition_MigrateLocalFeedsToCloud(UserInfo user, PermanentState permState)
        {
            this.user = user;
            this.permState = permState;
        }

        public async Task Transition()
        {
            if (permState.IsFirstTime)
            {

            }
            else
            {
                var dal = new weave.Data.Weave4DataAccessLayer();

                try
                {
                    // migrate the feeds
                    var existingFeeds = await dal.Feeds.Value;
                    user.Feeds = new ObservableCollection<Feed>(existingFeeds.Select(o =>
                        new Feed
                        {
                            Name = o.FeedName,
                            Uri = o.FeedUri,
                            ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
                            Category = o.Category,
                        }));
                    await user.Create();

                    // migrate any favorited articles
                    var favorites = existingFeeds.SelectMany(o => o.News).Where(o => o.IsFavorite);

                    foreach (var o in favorites)
                    {
                        await articleService.AddFavorite(
                            user.Id,
                            new SavedNewsItem
                            {
                                SourceName = o.OriginalSource,
                                Link = o.Link,
                                ImageUrl = o.ImageUrl,
                                UtcPublishDateTime = o.PublishDateTime.ToString(),
                                Title = o.Title,
                                PodcastUri = o.PodcastUri,
                                ZuneAppId = o.ZuneAppId,
                                VideoUri = o.VideoUri,
                                YoutubeId = o.YoutubeId,
                            });
                    }
                }
                catch 
                { 
                }
            }
        }
    }
}
