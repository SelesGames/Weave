using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ZuneCrawler.Core.Zest
{
    public static class ZestService
    {
        public static string NO_MARKER = "NOMARKER";
    }

    internal static class ZestParserExtensionMethods
    {
        static readonly XNamespace ns = "http://www.w3.org/2005/Atom";
        static readonly XNamespace zestns = "http://schemas.zune.net/catalog/apps/2008/02";

        public static IEnumerable<ZestAppData> GetZestApps(this XElement element)
        {
            return
                from e in element.Elements(ns + "entry")
                select new ZestAppData
                {
                    Title = e.Element(ns + "title").Value,

                    Id = Regex.Replace(e.Element(ns + "id").Value, "(urn:uuid:)(.)", "$2"),

                    ReleaseDate = DateTime.Parse(e.Element(zestns + "releaseDate").Value),

                    Updated = DateTime.Parse(e.Element(ns + "updated").Value),

                    ShortDescription = e.Element(zestns + "shortDescription") == null
                        ? "" : e.Element(zestns + "shortDescription").Value,

                    AverageUserRating = decimal.Parse(e.Element(zestns + "averageUserRating").Value),

                    UserRatingCount = int.Parse(e.Element(zestns + "userRatingCount").Value),

                    Version = e.Element(zestns + "version").Value,

                    ImageId = Regex.Replace(e.Element(zestns + "image").Element(zestns + "id").Value, "(urn:uuid:)(.)", "$2"),

                    Categories = (
                        from category in e.Elements(zestns + "categories").Elements(zestns + "category")
                        select new ZestCategory
                        {
                            Id = category.Element(zestns + "id").Value,
                            Title = category.Element(zestns + "title").Value,
                            IsRoot = category.Element(zestns + "isRoot").Value
                        }).ToList(),

                    Publisher = (
                        from publisher in e.Elements(zestns + "publisher")
                        select new ZestPublisher
                        {
                            Id = publisher.Element(zestns + "id").Value,
                            Name = publisher.Element(zestns + "name").Value
                        }).ToList(),

                    Offers = (
                        from offer in e.Elements(zestns + "offers").Elements(zestns + "offer")
                        select new ZestOffer
                        {
                            OfferId = offer.Element(zestns + "offerId").Value,
                            MediaInstanceId = offer.Element(zestns + "mediaInstanceId").Value,
                            Price = decimal.Parse(offer.Element(zestns + "price").Value),
                            PriceCurrencyCode = offer.Element(zestns + "priceCurrencyCode").Value,
                            LicenseRight = offer.Element(zestns + "licenseRight").Value,
                            PaymentType = (
                                from paymenttype in offer.Elements(zestns + "paymentTypes").Elements()
                                select paymenttype.Value).ToList()
                        }).ToList(),

                    Tags = (
                        from tag in e.Elements(zestns + "tags").Elements(zestns + "tag")
                        select tag.Value
                        ).ToList(),
                };
        }

        public static IEnumerable<ZestAppReview> GetZestAppReviews(this XElement element)
        {
            return
                from e in element.Elements(ns + "entry")
                select new ZestAppReview
                {
                    Updated = DateTime.Parse(e.Element(ns + "updated").Value),
                    
                    Content = e.Element(ns + "content").Value,

                    Author = e.Element(ns + "author").Value,

                    UserRating = decimal.Parse(e.Element(zestns + "userRating").Value),
                };
        }

        internal static string GetAfterMarker(this XElement element)
        {
            var afterMarker =
                from e in element.Elements(ns + "link")
                where e.Attribute("rel").Value == "next"
                select (string)e.Attribute("href").Value;

            if (afterMarker.Any())
            {
                var result = afterMarker.First();

                var am = GetAfterMarker(result);
                if (string.IsNullOrEmpty(am))
                    return ZestService.NO_MARKER;
                else
                    return am;
            }
            else
                return ZestService.NO_MARKER;
        }

        static string GetAfterMarker(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            var afterMarker = url
                .Split('?')
                .Skip(1)
                .SelectMany(o => o.Split('&'))
                .Where(o => o.StartsWith("afterMarker", StringComparison.OrdinalIgnoreCase))
                .Select(o => o.Split('=').Skip(1).FirstOrDefault())
                .FirstOrDefault();

            return afterMarker;
        }

        //public static string GetAfterMarker(this XElement element)
        //{
        //    var afterMarker =
        //        from e in element.Elements(ns + "link")
        //        where e.Attribute("rel").Value == "next"
        //        select (string)e.Attribute("href").Value;

        //    if (afterMarker.Any())
        //    {
        //        var result = afterMarker.First();
        //        return GetAfterMarker(result);
        //    }
        //    else
        //        return null;
        //}
    }
}
