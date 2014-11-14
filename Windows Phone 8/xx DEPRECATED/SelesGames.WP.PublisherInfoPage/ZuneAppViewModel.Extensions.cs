using System.Collections.Generic;
using System.Linq;

namespace SelesGames.WP.PublisherInfoPage
{
    internal static class ZuneAppViewModel_Extensions
    {
        public static ZuneAppViewModel ToZuneAppViewModel(this SelesGames.ZestLibrary.AppInfo app)
        {
            return new ZuneAppViewModel
            {
                AppId = app.Id,
                AppName = app.Title,
                Category = app.Category,
                UserRatingCount = app.UserRatingCount,
                Price = app.Price,
                Rating = app.AverageUserRating,
                OfferType = app.OfferType,
                Publisher = app.Publisher,
            };
        }

        public static List<ZuneAppViewModel> ToViewModels(this IEnumerable<SelesGames.ZestLibrary.AppInfo> apps)
        {
            return apps.Select(ToZuneAppViewModel).ToList();
        }

        //public static Task AllIconsLoadedAsync(this IEnumerable<ZuneAppViewModel> apps)
        //{
        //    return TaskEx.WhenAll(apps.Select(o => o.LoadAppIconAsync()));
        //}

        public static string DisplayableCategoryTypeName(this string categoryName)
        {
            if (categoryName == "xbox")
                return "xbox games";
            else if (categoryName == "indie")
                return "indie games";
            else if (categoryName == "games")
                return categoryName;
            else
                return categoryName + " apps";
        }
    }
}
