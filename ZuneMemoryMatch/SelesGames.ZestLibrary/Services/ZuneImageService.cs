
namespace SelesGames.ZestLibrary
{
    public class ZuneImageService
    {
        static string regionCode;
        static ZuneImageService()
        {
            regionCode = System.Globalization.CultureInfo.CurrentCulture.Name;
        }

        //static readonly string baseImageUrl = "http://image.catalog.zune.net/v3.2/en-US/image/{0}?width=160&height=120";
        static readonly string fromAppIdImageUrlSmall = "http://catalog.zune.net/v3.2/en-US/apps/{0}/primaryImage?width=60&height=60&resize=true";
        static readonly string fromAppIdImageUrlMedium = "http://catalog.zune.net/v3.2/en-US/apps/{0}/primaryImage?width=99&height=99&resize=true";


        static readonly string screenshotThumbnailUrl =
"http://catalog.zune.net/v3.2/{0}/image/{1}?width=120&resize=true&contenttype=image/jpeg";

        static readonly string screenshotFullSizeUrl =
"http://catalog.zune.net/v3.2/{0}/image/{1}?width=480&resize=true&contenttype=image/jpeg";


        //public static string CreateImageUrlFromImageId(string imageId)
        //{
        //    return string.Format(baseImageUrl, imageId);
        //}

        /// <summary>
        /// Creates a 60 x 60 image url for the given app ID
        /// </summary>
        /// <param name="appId"></param>
        /// <returns>The full image url</returns>
        public static string CreateSmallImageUrlFromAppId(string appId)
        {
            return string.Format(fromAppIdImageUrlSmall, appId);
        }

        /// <summary>
        /// Creates a 99 x 99 image url for the given app ID
        /// </summary>
        /// <param name="appId"></param>
        /// <returns>The full image url</returns>
        public static string CreateMediumImageUrlFromAppId(string appId)
        {
            return string.Format(fromAppIdImageUrlMedium, appId);
        }

        public static string CreateScreenshotThumbnailUrlFromImageId(string imageId)
        {
            return string.Format(screenshotThumbnailUrl, regionCode, imageId);
        }

        public static string CreateScreenshotFullSizeUrlFromImageId(string imageId)
        {
            return string.Format(screenshotFullSizeUrl, regionCode, imageId);
        }
    }
}
