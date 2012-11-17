using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace weave
{
    public class HtmlToStringListParser
    {
        List<StringBuilder> paragraphs = new List<StringBuilder>();
        StringBuilder currentParagraph;

        public IEnumerable<string> ToXaml(string html)
        {
            currentParagraph = new StringBuilder();
            paragraphs.Add(currentParagraph);
            try
            {
                HtmlDocument doc = new HtmlDocument();
                //doc.OptionDefaultStreamEncoding = Encoding.UTF8;
                doc.LoadHtml(html);

                ParseNode(doc.DocumentNode);

                var filtered = paragraphs
                    .SelectMany(sb => ScrubString(sb.ToString()))
                    .Where(s => !string.IsNullOrEmpty(s) && s != " ")
                    .ToList();

                return filtered;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        void ParseNode(HtmlNode node)
        {
            if (node.Name == "p")
            {
                currentParagraph = new StringBuilder();
                paragraphs.Add(currentParagraph);
            }

            if (node.ChildNodes == null || node.ChildNodes.Count <= 0)
            {
                currentParagraph.Append(node.InnerText);
            }
            else
            {
                foreach (var childNode in node.ChildNodes)
                {
                    ParseNode(childNode);
                }
            }
        }

        static Regex _tags = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        string RemoveRemainingHtmlFromString(string input)
        {
            //string tagname;
            Match tag;

            // match every HTML tag in the input
            MatchCollection tags = _tags.Matches(input);
            for (int i = tags.Count - 1; i > -1; i--)
            {
                tag = tags[i];
                //tagname = tag.Value.ToLowerInvariant();
                input = input.Remove(tag.Index, tag.Length);
            }
            return input;
        }

        IEnumerable<string> ScrubString(string input)
        {
            try
            {
                var htmlStrippedAndTrimmed = RemoveRemainingHtmlFromString(input.Trim());
                var decoded = HttpUtility.HtmlDecode(htmlStrippedAndTrimmed);
                var sr = new StringReader(decoded);

                List<string> lines = new List<string>();
                bool linesRemain = true;
                while (linesRemain)
                {
                    var line = sr.ReadLine();
                    if (line != null)
                    {
                        line = line.Trim();
                        lines.Add(line);
                    }
                    else
                        linesRemain = false;
                }
                return lines;
            }
            catch (Exception) { return new List<string>(); }
        }
    }
}
