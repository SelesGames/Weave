using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SelesGames.Rest;

namespace SelesGames.ZestLibrary
{
    #region AppTag (apptag.independent or apptag.premium)

    public enum AppTag
    {
        Independent,
        Premium
    }

    public static class AppTagExtensions
    {
        public static string ToQueryString(this AppTag appTag)
        {
            if (appTag == AppTag.Independent)
                return "apptag.independent";
            else if (appTag == AppTag.Premium)
                return "apptag.premium";
            else
                throw new Exception("invalid AppTag state!");
        }
    }

    #endregion




    #region AppOrder (downloadRank or releaseDate - though "Title" is valid, but not useful)

    public enum AppOrder
    {
        DownloadRank,
        ReleaseDate
    }

    public static class AppOrderExtensions
    {
        public static string ToQueryString(this AppOrder order)
        {
            if (order == AppOrder.DownloadRank)
                return "downloadRank";
            else if (order == AppOrder.ReleaseDate)
                return "releaseDate";
            else
                throw new Exception("invalid AppOrder!");
        }
    }

    #endregion




    #region AppCost (free or paid)

    public enum AppCost
    {
        Paid,
        Free
    }

    public static class AppCostExtensions
    {
        public static string ToQueryString(this AppCost appCost)
        {
            if (appCost == AppCost.Free)
                return "free";
            else if (appCost == AppCost.Paid)
                return "paid";
            else
                throw new Exception("invalid AppCost!");
        }
    }

    #endregion




    public class BrowseZuneResult
    {
        public string AfterMarker { get; set; }
        public List<AppInfo> Apps { get; set; }
    }

    public class BrowseReviewsResult
    {
        public string AfterMarker { get; set; }
        public List<AppReview> Reviews { get; set; }
    }

    public class ZuneService
    {
        //const string FEATURED_APPS = "http://catalog.zune.net/v3.2/en-US/clientTypes/WinMobile%207.0/hubTypes/apps/hub/?store=zest";
        static XNamespace a = "http://www.w3.org/2005/Atom";
        const string x = "http://schemas.zune.net/catalog/apps/2008/02";
        const string detailedInfoUri = "http://catalog.zune.net/v3.2/en-US/apps/{0}/?version=latest&clientType=WinMobile%207.0&store=Zest";
        //urn:uuid:be466f3f-8621-4126-99b7-bfe199f69c1a
        //urn:uuid:9fef739b-7589-40c8-9597-490461215692
        //http://catalog.zune.net/v3.2/en-US/apps/494a4f40-ac1f-e011-854c-00237de2db9e/?version=latest&clientType=WinMobile%207.0&store=Zest 

        public static string NO_MARKER = "NOMARKER";

        //readonly string baseBrowseAppsUrl = "http://catalog.zune.net/v3.2/{0}/appCategories/{1}/apps?clientType=WinMobile%207.1&store=Zest";


        readonly string getAppsBaseUrl = "http://catalog.zune.net/v3.2/{0}/apps";
        readonly string getAppsByCategoryBaseUrl = "http://catalog.zune.net/v3.2/{0}/appCategories/{1}/apps";
        //readonly string getAppsForPublisherBaseUrl = "http://catalog.zune.net/v3.2/{0}/publishers/{1}/apps";


        readonly string baseReviewsUrl = "http://catalog.zune.net/v3.2/{0}/apps/{1}/reviews";
        //"http://catalog.zune.net/v3.2/en-US/apps/bc3de69a-b7cd-df11-9eae-00237de2db9e/reviews?afterMarker=AAAABgAAn4oBSrE5TEC4vFQYRkWBQSPtc6TIqw%3d%3d&chunkSize=10&orderBy=Default";




        #region REVIEWS

        Uri CreateBrowseReviewsUrl(
            string regionCode,
            string appId,
            string afterMarker = null,
            int? chunkSize = null)
        {
            var uri = string.Format(baseReviewsUrl, regionCode, appId);

            var uriBuilder = new UriBuilder(uri);

            if (!string.IsNullOrEmpty(afterMarker))
                uriBuilder.AppendParameter("afterMarker", afterMarker);

            if (chunkSize.HasValue)
                uriBuilder.AppendParameter("chunkSize", chunkSize.ToString());

            uriBuilder.AppendParameter("orderBy", "Default");

            return uriBuilder.Uri;
        }

        public Task<BrowseReviewsResult> BrowseReviewsAsync(
            string regionCode,
            string appId,
            string afterMarker = null,
            int? chunkSize = null)
        {
            var uri = CreateBrowseReviewsUrl(regionCode, appId, afterMarker, chunkSize);

            var client = new LinqToXmlRestClient<BrowseReviewsResult> { UseGzip = true };
            return client.GetAndParseAsync(uri.OriginalString, xml =>
                {
                    var reviews = xml
                        .GetAppReviews()
                        .ToList();

                    var am = xml.GetAfterMarker();

                    if (reviews.Count < chunkSize)
                        am = NO_MARKER;

                    var result = new BrowseReviewsResult { Reviews = reviews, AfterMarker = am };
                    return result;
                }, CancellationToken.None);
        }

        #endregion




        #region BROWS AND SEARCH APPS

        Uri CreateBrowseAppsUrl(
            string regionCode, 
            string categoryId = null, 
            string query = null,
            AppTag? tag = null, 
            string publisher = null,
            string afterMarker = null, 
            int? chunkSize = null, 
            AppOrder? orderBy = null, 
            AppCost? cost = null)
        {
            string uri;

            if (string.IsNullOrEmpty(categoryId))
                uri = string.Format(getAppsBaseUrl, regionCode);
            else
            {
                if (!string.IsNullOrEmpty(publisher))
                    throw new ArgumentException("You cannot call the Zune service with both a publisher and a categoryId specified at the same time");

                uri = string.Format(getAppsByCategoryBaseUrl, regionCode, categoryId);
            }

            var uriBuilder = new UriBuilder(uri);

            if (!string.IsNullOrEmpty(query))
                uriBuilder.AppendParameter("q", query);

            if (tag.HasValue)
                uriBuilder.AppendParameter("tag", tag.Value.ToQueryString());

            if (!string.IsNullOrEmpty(publisher))
                uriBuilder.AppendParameter("publisher", publisher);

            if (!string.IsNullOrEmpty(afterMarker))
                uriBuilder.AppendParameter("afterMarker", afterMarker);

            if (chunkSize.HasValue)
                uriBuilder.AppendParameter("chunkSize", chunkSize.ToString());

            if (orderBy.HasValue)
                uriBuilder.AppendParameter("orderBy", orderBy.Value.ToQueryString());

            if (cost.HasValue)
                uriBuilder.AppendParameter("cost", cost.Value.ToQueryString());


            uriBuilder.AppendParameter("store", "Zest");
            uriBuilder.AppendParameter("clientType", "WinMobile+7.1");

            return uriBuilder.Uri;
        }

        public Task<BrowseZuneResult> BrowseAppsAsync(
            string regionCode,
            string categoryId = null,
            string query = null,
            AppTag? tag = null,
            string publisher = null,
            string afterMarker = null,
            int? chunkSize = null,
            AppOrder? orderBy = null,
            AppCost? cost = null)
        {
            var uri = CreateBrowseAppsUrl(regionCode, categoryId, query, tag, publisher, afterMarker, chunkSize, orderBy, cost);

            var client = new LinqToXmlRestClient<BrowseZuneResult> { UseGzip = true };
            return client.GetAndParseAsync(uri.OriginalString, xml =>
                {
                    var apps = xml
                        .GetApps()
                        .ToList();

                    var am = xml.GetAfterMarker();

                    var result = new BrowseZuneResult { Apps = apps, AfterMarker = am };
                    return result;
                }, CancellationToken.None);
        }

        #endregion




        #region Extended app info (DESCRIPTION AND SCREENSHOTS)

        public static Task<ExtendedZuneAppInfo> GetExtendedZuneAppInfo(string appId)
        {
            var uri = string.Format(detailedInfoUri, appId);

            var client = new LinqToXmlRestClient<ExtendedZuneAppInfo> { UseGzip = true };
            return client.GetAndParseAsync(uri, xml =>
                {
                    var extendedInfo =
                        new ExtendedZuneAppInfo
                        {
                            Description = xml.Element(a + "content").ValueOrDefault(),
                            //ShortDescription = element.Element(XName.Get("shortDescription", x)).ValueOrDefault(),
                            //Publisher = element.Element(XName.Get("publisher", x)).ValueOrDefault(),
                            //UserRating = element.Element(XName.Get("averageUserRating", x)).ValueOrDefault(),
                            //Price = element
                            //            .Element(XName.Get("offers", x))
                            //            .Elements(XName.Get("offer", x))
                            //            .Where(o => o.Element(XName.Get("licenseRight", x)).ValueOrDefault() == "Purchase")
                            //            .SingleOrDefault()
                            //            .Element(XName.Get("displayPrice", x)).ValueOrDefault(),
                            //BackgroundImage = Try(() => ExtractImageUrl(element.Element(XName.Get("backgroundImage", x)).Element(XName.Get("id", x)).ValueOrDefault()), string.Empty),
                            ScreenShotIds = xml
                                .Element(XName.Get("screenshots", x))
                                .Elements(XName.Get("screenshot", x))
                                .Select(o => ExtractImageId(o.Element(XName.Get("id", x)).ValueOrDefault()))
                                .ToList()
                        };

                    return extendedInfo;
                }, CancellationToken.None);
        }

        static string ExtractImageId(string p)
        {
            if (p == null)
                return null;

            return p.Replace("urn:uuid:", string.Empty);
        }

        #endregion
    }

    public class ExtendedZuneAppInfo
    {
        public string Description { get; set; }
        //public string ShortDescription { get; set; }
        //public string Publisher { get; set; }
        //public string UserRating { get; set; }
        //public string Price { get; set; }
        //public string BackgroundImage { get; set; }
        public List<string> ScreenShotIds { get; set; }
    }
}
