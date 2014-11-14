using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

namespace Weave.Microsoft.OneNote
{
    public class Formatter
    {
        #region Private member variables

        const string MOBILIZED_TEMPLATE_PATH = "/Weave.Services.Extra;component/Templates/MobilizedHtml.txt";
        const string UNMOBILIZED_TEMPLATE_PATH = "/Weave.Services.Extra;component/Templates/UnmobilizedHtml.txt";
        
        const string HERO_IMAGE_HTML_TEMPLATE = 
"<div><a href=\"{0}\"><img src=\"{0}\"/></a></div>";

        bool areTemplatesLoaded = false;

        string
            mobilizedTemplate,
            staticTemplate;

        #endregion




        public Encoding Encoding { get; set; }

        public Formatter()
        {
            Encoding = new UTF8Encoding(false, false);
        }

        public async Task<string> CreateHtml(MobilizedOneNoteItem oneNoteItem)
        {
            if (!areTemplatesLoaded)
                await ReadHtmlTemplate();

            string title, link, sourceAndPubdate, heroImage, body;

            title = oneNoteItem.Title;
            link = oneNoteItem.Link;
            sourceAndPubdate = oneNoteItem.Source;
            body = oneNoteItem.BodyHtml;
            heroImage = string.IsNullOrWhiteSpace(oneNoteItem.HeroImage) ?
                null : string.Format(HERO_IMAGE_HTML_TEMPLATE, oneNoteItem.HeroImage);

            var sb = new StringBuilder();

            sb
                .AppendLine(
                    new StringBuilder(mobilizedTemplate)
                        .Replace("[TITLE]", title)
                        .Replace("[HEROIMAGE]", heroImage)
                        .Replace("[SOURCE]", sourceAndPubdate)
                        .Replace("[LINK]", link)
                        .Replace("[BODY]", body)
                        .ToString());

            return sb.ToString();
        }

        public async Task<string> CreateHtml(HtmlLinkOneNoteItem oneNoteItem)
        {
            if (!areTemplatesLoaded)
                await ReadHtmlTemplate();

            string title, link, sourceAndPubdate;

            title = oneNoteItem.Title;
            link = oneNoteItem.Link;
            sourceAndPubdate = oneNoteItem.Source;

            var sb = new StringBuilder();

            sb
                .AppendLine(
                    new StringBuilder(staticTemplate)
                        .Replace("[TITLE]", title)
                        .Replace("[SOURCE]", sourceAndPubdate)
                        .Replace("[LINK]", link)
                        .ToString());

            return sb.ToString();
        }




        #region Private Helper Methods (read the html from resources)

        async Task ReadHtmlTemplate()
        {
            mobilizedTemplate = await LoadTemplate(MOBILIZED_TEMPLATE_PATH);
            staticTemplate = await LoadTemplate(UNMOBILIZED_TEMPLATE_PATH);

            areTemplatesLoaded = true;
        }

        async Task<string> LoadTemplate(string templatePath)
        {
            Uri uri = new Uri(templatePath, UriKind.Relative);
            StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
            using (StreamReader streamReader = new StreamReader(streamResourceInfo.Stream, Encoding))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        #endregion
    }
}
