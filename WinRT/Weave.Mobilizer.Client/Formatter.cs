using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Weave.Mobilizer.Client
{
    public class Formatter
    {
        const string HTML_TEMPLATE_PATH1 = "ms-appx:///Templates/html_template1.txt";
        const string HTML_TEMPLATE_PATH2 = "ms-appx:///Templates/html_template2.txt";
        const string HTML_TEMPLATE_PATH3 = "ms-appx:///Templates/html_template3.txt";
        const string CSS_TEMPLATE_PATH = "ms-appx:///Templates/css_template.txt";
        const string BODY_TEMPLATE_PATH = "ms-appx:///Templates/body_template.txt";

        bool areTemplatesLoaded = false;

        string htmlTemplate1;
        string htmlTemplate2;
        string htmlTemplate3;
        string cssTemplate;
        string bodyTemplate;

        public Encoding Encoding { get; set; }

        public Formatter()
        {
            Encoding = new UTF8Encoding(false, false);
        }

        public string CreateHtml(
            string source, 
            string title, 
            string link,
            string body, 
            string foreground, 
            string background, 
            string fontName, 
            string fontSize, 
            string linkColor)
        {
            if (!areTemplatesLoaded)
                ReadHtmlTemplate();

            var sb = new StringBuilder();
                
            sb
                .AppendLine(htmlTemplate1)

                .AppendLine(
                    new StringBuilder(cssTemplate)
                        .Replace("[FOREGROUND]", foreground)
                        .Replace("[BACKGROUND]", background)
                        .Replace("[FONT]", fontName)
                        .Replace("[FONTSIZE]", fontSize)
                        .Replace("[ACCENT]", linkColor)
                        .ToString())

                .AppendLine(htmlTemplate2)

                .AppendLine(
                    new StringBuilder(bodyTemplate)
                        .Replace("[SOURCE]", source)
                        .Replace("[TITLE]", title)
                        .Replace("[LINK]", link)
                        .Replace("[BODY]", body)
                        .ToString())

                .AppendLine(htmlTemplate3);

            return sb.ToString();
        }

        void ReadHtmlTemplate()
        {
            LoadTemplate(HTML_TEMPLATE_PATH1, out htmlTemplate1);
            LoadTemplate(HTML_TEMPLATE_PATH2, out htmlTemplate2);
            LoadTemplate(HTML_TEMPLATE_PATH3, out htmlTemplate3);
            LoadTemplate(CSS_TEMPLATE_PATH, out cssTemplate);
            LoadTemplate(BODY_TEMPLATE_PATH, out bodyTemplate);
            areTemplatesLoaded = true;
        }

        void LoadTemplate(string templatePath, out string template)
        {
            var file = StorageFile.GetFileFromApplicationUriAsync(new Uri(templatePath, UriKind.Absolute)).AsTask().WaitOnResult();
            template = FileIO.ReadTextAsync(file).AsTask().WaitOnResult();
        }


    }

    internal static class TaskExtensions
    {
        public static T WaitOnResult<T>(this Task<T> task)
        {
            if (task.IsCompleted)
                return task.Result;

            else
            {
                task.Wait();
                return task.Result;
            }
        }
    }
}
