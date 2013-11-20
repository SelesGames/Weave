using System.Net;
using System.Text.RegularExpressions;

namespace weave
{
    internal class MobilizedArticle
    {
        public string Title { get; set; }
        public string Publication { get; set; }
        public string Date { get; set; }
        public string Content { get; set; }

        public string CreateSpokenRepresentation()
        {
            var title = Title.Trim();
            var content = HttpUtility.HtmlDecode(Sanitize(Content).Trim()).Trim();

            var fullText = string.Format(
                "{0}.\r\n\r\nPublished by {1}.\r\n\r\n{2}",
                title,
                Publication,
                content);

            return fullText;
        }

        static Regex _tags = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            string tagname;
            Match tag;

            // match every HTML tag in the input
            MatchCollection tags = _tags.Matches(html);
            for (int i = tags.Count - 1; i > -1; i--)
            {
                tag = tags[i];
                tagname = tag.Value.ToLowerInvariant();

                html = html.Remove(tag.Index, tag.Length);
            }

            return html;
        }
    }
}
