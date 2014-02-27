using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ZuneCrawler.Core.Zest;

namespace SelesGames.ZestLibrary
{
    public static class XmlToAppsParser
    {
        internal static IEnumerable<AppInfo> GetApps(this XElement element)
        {
            return element.GetZestApps()
                .Select(o => o.ToZuneAppInfo())
                .Select(app => new AppInfo
                {
                    Id = app.Id,
                    Title = app.Title,
                    Category = app.Category,
                    UserRatingCount = app.UserRatingCount,
                    OfferType = app.OfferType,
                    Price = app.Price,
                    AverageUserRating = app.AverageUserRating,
                    Publisher = app.Publisher,
                    ImageId = app.ImageId,
                    ReleaseDate = app.ReleaseDate,
                    Version = app.Version,
                    IsXboxLive = app.IsXboxLive,
                    ShortDescription = app.ShortDescription,
                });
        }

        internal static string GetAfterMarker(this XElement element)
        {
            return ZuneCrawler.Core.Zest.ZestParserExtensionMethods.GetAfterMarker(element);
        }

        internal static IEnumerable<AppReview> GetAppReviews(this XElement element)
        {
            return element.GetZestAppReviews().Select(o => new AppReview
            {
                Author = o.Author,
                Content = o.Content,
                Updated = o.Updated,
                UserRating = o.UserRating
            });
        }
    }
}
