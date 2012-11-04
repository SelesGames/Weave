using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;


namespace weave.Services.RSS
{
    public static class RssServiceLayer
    {
        public static IEnumerable<XElement> ToXElements(this Stream stream)
        {
            try
            {
                List<XElement> elements = new List<XElement>();
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    reader.ReadToDescendant("item");

                    using (var newReader = reader.ReadSubtree())
                    {
                        var itemElement = XElement.Load(newReader);
                        elements.Add(itemElement);
                    }

                    while (reader.ReadToNextSibling("item"))
                    {
                        using (var newReader = reader.ReadSubtree())
                        {
                            var itemElement = XElement.Load(newReader);
                            elements.Add(itemElement);
                        }
                    }
                }
                return elements;
            }
            catch (Exception)
            {
                return new List<XElement>();
            }
        }

        public static IEnumerable<Tuple<DateTime, XElement>> ToXElementsWithLocalDate(this IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var dateTime = TryGetLocalDate(element);
                if (dateTime.Item1)
                    yield return Tuple.Create(dateTime.Item2, element);
            }
        }

        static Tuple<bool, DateTime> TryGetLocalDate(XElement item)
        {
            var pubDate = item.Element("pubDate").ValueOrDefault();
            return TryGetLocalDate(pubDate);
        }

        public static Tuple<bool, DateTime> TryGetLocalDate(string dateTimeString)
        {
            try
            {
                if (string.IsNullOrEmpty(dateTimeString))
                    return Tuple.Create(false, DateTime.MinValue);

                DateTime dateTime;

                var canRfcParse = SyndicationDateTimeUtility
                    .TryParseRfc822DateTime(dateTimeString, out dateTime);

                if (!canRfcParse)
                {
                    var canNormalParse = DateTime.TryParse(dateTimeString, out dateTime);
                    if (!canNormalParse)
                    {
                        string canParseAny = new[] { "ddd, dd MMM yyyy HH:mm:ss ZK", "yyyy-MM-ddTHH:mm:ssK" }
                            .FirstOrDefault(o => DateTime.TryParseExact(dateTimeString, o, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));

                        if (canParseAny == null)
                            return Tuple.Create(false, DateTime.MinValue);
                    }
                }

                if (dateTime.Kind == DateTimeKind.Unspecified || dateTime.Kind == DateTimeKind.Utc)
                {
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
                }

                if (dateTime > DateTime.Now)
                    dateTime = DateTime.Now;

                return Tuple.Create(true, dateTime);
            }
            catch (Exception)
            {
                return Tuple.Create(false, DateTime.MinValue);
            }
        }


        public static NewsItem ToNewsItem(this XElement item, DateTime pubDate)
        {
            try
            {
                var title = item.Element("title");
                var link = item.Element("link");

                if (title == null || link == null)
                    return null;

                XElement description;
                var content = getContentNode(item);
                if (content != null)
                    description = content;
                else
                    description = item.Element("description");

                string imageUrl;
                var hasImage = TryParseImageUrl(item, description, out imageUrl);

                return new NewsItem
                {
                    Title = System.Net.HttpUtility.HtmlDecode(title.Value.Trim()),
                    Link = link.Value,
                    Description = description.ValueOrDefault(),
                    PublishDateTime = pubDate,//item.Element("pubDate").ValueOrDefault(),
                    ImageUrl = imageUrl,
                };
            }
            catch (Exception)
            {
                return null;
            }
        }


        static bool TryParseImageUrl(XElement item, XElement description, out string result)
        {
            var thumb = parseAnyImageInTheDescription(description);

            if (string.IsNullOrEmpty(thumb))
                thumb = parseMediaContentUrl(item);

            if (string.IsNullOrEmpty(thumb))
                thumb = parseEnclosure(item);

            if (!string.IsNullOrEmpty(thumb))
                thumb = Uri.EscapeUriString(thumb);

            result = thumb;

            return !string.IsNullOrEmpty(thumb) &&
                Uri.IsWellFormedUriString(thumb, UriKind.Absolute);
        }

        static string parseEnclosure(XElement item)
        {
            var mediaTag = item.Element("enclosure");
            if (mediaTag == null)
                return null;

            var thumbUrlTag = mediaTag.Attribute("url");
            if (thumbUrlTag == null)
                return null;

            return thumbUrlTag.Value;
        }

        static readonly XNamespace media = "http://search.yahoo.com/mrss/";
        static string parseMediaContentUrl(XElement item)
        {
            var mediaTag = item.Element(media + "content");
            if (mediaTag == null)
                return null;

            var thumbUrlTag = mediaTag.Attribute("url");
            if (thumbUrlTag == null)
                return null;

            return thumbUrlTag.Value;
        }

        static readonly XNamespace content = "http://purl.org/rss/1.0/modules/content/";
        static XElement getContentNode(XElement item)
        {
            return item.Element(content + "encoded");
        }

        static string parseAnyImageInTheDescription(XElement item)
        {
            if (item == null)
                return null;

            var description = item.Value;

            return parseImageUrlFromHtml(description);
        }

        static string parseImageUrlFromHtml(string html)
        {
            Regex r = new Regex(@"src=(?:\""|\')?(?<imgSrc>[^>]*[^/].(?:jpg|png))(?:\""|\')?");

            Match m = r.Match(html);
            if (m.Success && m.Groups.Count > 1 && m.Groups[1].Captures.Count > 0)
            {
                return m.Groups[1].Captures[0].Value;
            }

            return null;
        }

        //public static IEnumerable<string> Sanitize(string html)
        //{
        //    if (String.IsNullOrEmpty(html)) return new List<string>();

        //    var stopwatch = Stopwatch.StartNew();

        //    var parsed = new HtmlToStringListParser().ToXaml(html);

        //    stopwatch.Stop();
        //    DebugEx.WriteLine("Took {0} milliseconds to load the HTML", stopwatch.ElapsedMilliseconds);

        //    //return html;
        //    return parsed;
        //}
    }
}