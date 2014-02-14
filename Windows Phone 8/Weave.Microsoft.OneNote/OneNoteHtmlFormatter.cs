using System;

namespace Weave.Microsoft.OneNote
{
    internal class OneNoteHtmlFormatter
    {
        const string TEMPLATE =
"<html>" +
"<head>" +
"<title>{0}</title>" +
"<meta name=\"created\" content=\"{1}\" />" +
"</head>" +
"<body>" +
"<h3>{2}</h3>" + 
"<a href=\"{3}\"><img data-render-src=\"{3}\" alt=\"{0}\" height=\"600\" width=\"480\" /></a>" +
"</body>" +
"</html>";

        public string CreateHtml(HtmlLinkOneNoteItem oneNoteItem)
        {
            var timestamp = DateTime.UtcNow.ToString();

            return string.Format(TEMPLATE,
                oneNoteItem.Title,
                timestamp,
                oneNoteItem.Source,
                oneNoteItem.Link);
        }
    }
}
