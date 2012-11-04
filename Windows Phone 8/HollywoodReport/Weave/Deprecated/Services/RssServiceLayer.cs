using System;
using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Xml;
//using System.Xml.Linq;

namespace weave
{
    public class RssServiceLayer
    {
        //public static double ImageFavoringWeight { get; set; }
        //const int respite = 1;

        //static RssServiceLayer()
        //{
        //    ImageFavoringWeight = 1.3;
        //}

        //public static IEnumerable<XElement> SafeParse(Stream stream)
        //{
        //    try
        //    {
        //        List<XElement> elements = new List<XElement>();
        //        using (XmlReader reader = XmlReader.Create(stream))
        //        {
        //            reader.ReadToDescendant("item");

        //            using (var newReader = reader.ReadSubtree())
        //            {
        //                var itemElement = XElement.Load(newReader);
        //                elements.Add(itemElement);
        //            }
        //            Thread.Sleep(respite);

        //            while (reader.ReadToNextSibling("item"))
        //            {
        //                using (var newReader = reader.ReadSubtree())
        //                {
        //                    var itemElement = XElement.Load(newReader);
        //                    elements.Add(itemElement);
        //                }
        //                Thread.Sleep(respite);
        //            }
        //        }
        //        return elements;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Debug.WriteLine(ex);
        //        return new List<XElement>();
        //    }
        //}

        //public static IEnumerable<Tuple<NewsItem, XElement>> FastParse(Stream stream)
        //{
        //    return SafeParse(stream)
        //        .Select(item =>
        //        {
        //            Thread.Sleep(respite);
        //            var link = item.Element("link");
        //            if (link == null)
        //                return null;

        //            return Tuple.Create(
        //                new NewsItem
        //                {
        //                    Link = link.Value,
        //                },
        //                item);
        //        })
        //        .OfType<Tuple<NewsItem, XElement>>();
        //}

        //public static IEnumerable<NewsItem> FastParseCompleteParsing(FeedSource feed, IEnumerable<Tuple<NewsItem, XElement>> input)
        //{
        //    if (input == null)
        //        return null;

        //    return input
        //        .Select(item => item.Item2)
        //        .Select(item =>
        //        {
        //            Thread.Sleep(respite);
        //            return ParseToNewsItem(feed, item);
        //        })
        //        .OfType<NewsItem>();
        //}

        //public static NewsItem ParseToNewsItem(FeedSource feed, XElement item)
        //{
        //    var pubDate = item.Element("pubDate").ValueOrDefault();
        //    if (string.IsNullOrEmpty(pubDate))
        //        return null;

        //    DateTime dateTime;
        //    var canRfcParse = SyndicationDateTimeUtility
        //        .TryParseRfc822DateTime(pubDate, out dateTime);

        //    if (!canRfcParse)
        //    {
        //        var canNormalParse = DateTime.TryParse(pubDate, out dateTime);
        //        if (!canNormalParse)
        //        {
        //            string canParseAny = new[] { "ddd, dd MMM yyyy HH:mm:ss ZK", "yyyy-MM-ddTHH:mm:ssK" }
        //                .FirstOrDefault(o => DateTime.TryParseExact(pubDate, o, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));

        //            if (canParseAny == null)
        //                return null;
        //        }
        //    }

        //    if (dateTime.Kind == DateTimeKind.Unspecified ||
        //        dateTime.Kind == DateTimeKind.Utc)
        //    {
        //        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
        //    }

        //    if (dateTime > DateTime.Now)
        //        dateTime = DateTime.Now;

        //    var title = item.Element("title");
        //    var link = item.Element("link");

        //    if (title == null || link == null)
        //        return null;

        //    XElement description;
        //    var content = getContentNode(item);
        //    if (content != null)
        //        description = content;
        //    else
        //        description = item.Element("description");

        //    string imageUrl;
        //    var hasImage = TryParseImageUrl(item, description, out imageUrl);

        //    return new NewsItem
        //    {
        //        Title = HttpUtility.HtmlDecode(title.Value.Trim()),
        //        Link = link.Value,
        //        Description = description.ValueOrDefault(),
        //        PublishDateTime = dateTime,
        //        ImageUrl = imageUrl,
        //        FeedSource = feed,
        //    };
        //}


        //static bool TryParseImageUrl(XElement item, XElement description, out string result)
        //{
        //    var thumb = parseAnyImageInTheDescription(description);

        //    if (string.IsNullOrEmpty(thumb))
        //        thumb = parseMediaContentUrl(item);

        //    if (string.IsNullOrEmpty(thumb))
        //        thumb = parseEnclosure(item);

        //    if (!string.IsNullOrEmpty(thumb))
        //        thumb = Uri.EscapeUriString(thumb);

        //    result = thumb;

        //    return !string.IsNullOrEmpty(thumb) &&
        //        Uri.IsWellFormedUriString(thumb, UriKind.Absolute);
        //}

        //public static string parseEnclosure(XElement item)
        //{
        //    var mediaTag = item.Element("enclosure");
        //    if (mediaTag == null)
        //        return null;

        //    var thumbUrlTag = mediaTag.Attribute("url");
        //    if (thumbUrlTag == null)
        //        return null;

        //    return thumbUrlTag.Value;
        //}

        //static readonly XNamespace media = "http://search.yahoo.com/mrss/";
        //public static string parseMediaContentUrl(XElement item)
        //{
        //    var mediaTag = item.Element(media + "content");
        //    if (mediaTag == null)
        //        return null;

        //    var thumbUrlTag = mediaTag.Attribute("url");
        //    if (thumbUrlTag == null)
        //        return null;

        //    return thumbUrlTag.Value;
        //}

        //static readonly XNamespace content = "http://purl.org/rss/1.0/modules/content/";
        //public static XElement getContentNode(XElement item)
        //{
        //    return item.Element(content + "encoded");
        //}

        //static string parseAnyImageInTheDescription(XElement item)
        //{
        //    if (item == null)
        //        return null;

        //    var description = item.Value;

        //    return parseImageUrlFromHtml(description);
        //}

        //static string parseImageUrlFromHtml(string html)
        //{
        //    //Regex r = new Regex(@"<img .* src=\""(.*\.jpg)\""");#<\s*img [^\>]*src\s*=\s*(["\'])(.*?)\1#im
        //    //string pattern = @"<img\ssrc=""(?<pic>[^""]*)""\salt=""(?<alt>[^""]*)";
        //    //Regex r = new Regex(@"<img .* src=\""(.*\.jpg).*\""");

        //    Regex r = new Regex(@"src=(?:\""|\')?(?<imgSrc>[^>]*[^/].(?:jpg|png))(?:\""|\')?");
        //    //Regex r = new Regex(pattern);
        //    // Regex r = new Regex(@"<img.*?src=""(.*?)""", RegexOptions.IgnoreCase);
        //    Match m = r.Match(html);
        //    if (m.Success && m.Groups.Count > 1 && m.Groups[1].Captures.Count > 0)
        //    {
        //        return m.Groups[1].Captures[0].Value;
        //    }

        //    //r = new Regex(@"src=\""(http://.*\.jpg)");
        //    //m = r.Match(description);
        //    //if (m.Success && m.Groups.Count > 1 && m.Groups[1].Captures.Count > 0)
        //    //{
        //    //    return m.Groups[1].Captures[0].Value;
        //    //}

        //    //r = new Regex(@"src=\""(http://.*\.png)");
        //    //m = r.Match(description);
        //    //if (m.Success && m.Groups.Count > 1 && m.Groups[1].Captures.Count > 0)
        //    //{
        //    //    return m.Groups[1].Captures[0].Value;
        //    //}
        //    return null;
        //}

//        private static Regex _whitelist = new Regex(@"
//    ^</?(b(lockquote)?|code|d(d|t|l|el)|em|h(1|2|3)|i|kbd|li|ol|p(re)?|s(ub|up|trong|trike)?|ul)>$|
//    ^<(b|h)r\s?/?>$",
//            RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
//        private static Regex _whitelist_a = new Regex(@"
//    ^<a\s
//    href=""(\#\d+|(https?|ftp)://[-a-z0-9+&@#/%?=~_|!:,.;\(\)]+)""
//    (\stitle=""[^""<>]+"")?\s?>$|
//    ^</a>$",
//            RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
//        private static Regex _whitelist_img = new Regex(@"
//    ^<img\s
//    src=""https?://[-a-z0-9+&@#/%?=~_|!:,.;\(\)]+""
//    (\swidth=""\d{1,3}"")?
//    (\sheight=""\d{1,3}"")?
//    (\salt=""[^""<>]*"")?
//    (\stitle=""[^""<>]*"")?
//    \s?/?>$",
//            RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);


        /// <summary>
        /// sanitize any potentially dangerous tags from the provided raw HTML input using 
        /// a whitelist based approach, leaving the "safe" HTML tags
        /// CODESNIPPET:4100A61A-1711-4366-B0B0-144D1179A937
        /// </summary>
        public static IEnumerable<string> Sanitize(string html)
        {
            if (String.IsNullOrEmpty(html)) return new List<string>();

            //var stopwatch = Stopwatch.StartNew();

            var parsed = new HtmlToStringListParser().ToXaml(html);

            //stopwatch.Stop();
            //DebugEx.WriteLine("Took {0} milliseconds to load the HTML", stopwatch.ElapsedMilliseconds);

            //return html;
            return parsed;
        }
    }
}
