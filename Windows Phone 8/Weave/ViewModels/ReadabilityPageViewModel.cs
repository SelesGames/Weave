﻿using SelesGames;
using System;
using System.Text;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.SavedState;
using Weave.Settings;
using Weave.ViewModels;

namespace Weave.WP.ViewModels
{
    public class ReadabilityPageViewModel
    {
        Client client;
        Formatter formatter;
        StandardThemeSet themes;
        FontSizes fontSizes;
        PermanentState permState;
        MobilizerResult mobilizerResult;

        public NewsItem NewsItem { get; set; }
        public ArticleViewingType LastViewingType { get; set; }
        internal MobilizedArticle CurrentMobilizedArticle { get; private set; }

        public ReadabilityPageViewModel()
        {
            client = new Client();
            formatter = ServiceResolver.Get<Formatter>();
            themes = AppSettings.Instance.Themes;
            fontSizes = new FontSizes();
            permState = ServiceResolver.Get<PermanentState>();
        }

        public async Task<string> GetMobilizedArticleHtml()
        {
            if (NewsItem == null)
                throw new ArgumentNullException("NewsItem in ReadabilityPageViewModel");

            string html = null;

            try
            {
                if (NewsItem.Feed != null)
                {
                    var articleViewingType = NewsItem.Feed.ArticleViewingType;
                    LastViewingType = articleViewingType;
                    await GetMobilizerResult().ConfigureAwait(false);
                    html = GetMobilizedHtml();
                    CurrentMobilizedArticle = new MobilizedArticle 
                    { 
                        Title = NewsItem.Title,
                        Publication = NewsItem.Feed.Name,
                        Date = NewsItem.PublishDate,
                        Content = mobilizerResult.content
                    };
                }
            }
            catch 
            {
                CurrentMobilizedArticle = null;
                throw;
            }

            var convertedHtml = ConvertExtendedASCII(html);
            return convertedHtml;
        }

        async Task GetMobilizerResult()
        {
            mobilizerResult = await client.GetAsync(NewsItem.Link).ConfigureAwait(false);
        }

        string GetMobilizedHtml()
        {
            var theme = themes.CurrentTheme;

            var foreground = theme.Text;
            var background = theme.Background;
            var linkColor = theme.Accent;

            var title = NewsItem.Title;
            var link = NewsItem.Link;
            //var result = await client.GetAsync(NewsItem.Link).ConfigureAwait(false);
            var content = mobilizerResult.content;
            var heroImage = SelectBestImage();

            string source, pubDate;

            if (string.IsNullOrWhiteSpace(heroImage))
            {
                source = NewsItem.FormattedForMainPageSourceAndDate;
                pubDate = null;
            }
            else
            {
                source = NewsItem.OriginalSource.ToUpperInvariant();
                //pubDate = NewsItem.PublishDate;
                pubDate = NewsItem.LocalDateTime.ToLongDateString();
            }
                


            var fontSize = fontSizes.GetById(permState.ArticleViewFontSize).HtmlTextSize();

            var html = formatter.CreateHtml(source, title, pubDate, link, heroImage, content, foreground, background, permState.ArticleViewFontName, fontSize, linkColor);
            return html;
        }

        string SelectBestImage()
        {
            if (!string.IsNullOrWhiteSpace(mobilizerResult.lead_image_url))
                return mobilizerResult.lead_image_url;

            if (NewsItem.HasImage)
                return NewsItem.HighestQualityImageUrl;

            return null;
        }

        /// <summary>
        /// Converts UTF-8 to Unicode
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static string ConvertExtendedASCII(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var answer = new StringBuilder();
            char[] s = text.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127)
                    answer.Append("&#" + Convert.ToInt32(c) + ";");
                else
                    answer.Append(c);
            }
            var result = answer.ToString();
            return result;
        }
    }
}
