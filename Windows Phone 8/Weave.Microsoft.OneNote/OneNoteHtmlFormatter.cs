using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
"<body><h3>{3}</h3></body>" +
"</html>";

        public string CreateHtml(HtmlLinkOneNoteItem oneNoteItem)
        {
            return "";
        }
    }
}
