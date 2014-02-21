using SelesGames;
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
        Formatter formatter;
        StandardThemeSet themes;
        FontSizes fontSizes;
        PermanentState permState;

        public NewsItem NewsItem { get; set; }
        public ArticleViewingType LastViewingType { get; set; }
        internal MobilizedArticle CurrentMobilizedArticle { get; private set; }

        public ReadabilityPageViewModel()
        {
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

                    CurrentMobilizedArticle = await new MMMediator(NewsItem).DoStuff();
                    html = GetMobilizedHtml();
                }
            }
            catch 
            {
                CurrentMobilizedArticle = null;
                throw;
            }

            var convertedHtml = html.ConvertExtendedASCII();
            return convertedHtml;
        }

        string GetMobilizedHtml()
        {
            var theme = themes.CurrentTheme;

            var foreground = theme.Text;
            var background = theme.Background;
            var linkColor = theme.Accent;

            var title = NewsItem.Title;
            var link = NewsItem.Link;

            var content = CurrentMobilizedArticle.ContentHtml;// mobilizerResult.content;
            var heroImage = CurrentMobilizedArticle.HeroImageUrl;// SelectBestImage();

            string source, pubDate;

            if (!CurrentMobilizedArticle.HasImage)
            {
                source = CurrentMobilizedArticle.CombinedPublicationAndDate;
                pubDate = null;
            }
            else
            {
                source = CurrentMobilizedArticle.Source.ToUpperInvariant();
                pubDate = CurrentMobilizedArticle.FullDate;
            }

            var fontSize = fontSizes.GetById(permState.ArticleViewFontSize).HtmlTextSize();

            var html = formatter.CreateHtml(source, title, pubDate, link, heroImage, content, foreground, background, permState.ArticleViewFontName, fontSize, linkColor);
            return html;
        }
    }


    internal class MMMediator
    {
        NewsItem newsItem;

        public MMMediator(NewsItem newsItem)
        {
            this.newsItem = newsItem;
        }

        public async Task<MobilizedArticle> DoStuff()
        {
            var client = new Client();
            var mobilizerResult = await client.GetAsync(newsItem.Link);
            var coalescer = new ResultCombiner(newsItem, mobilizerResult);
            return coalescer.Combine();
        }
    }

    internal class ResultCombiner
    {
        NewsItem newsItem;
        MobilizerResult mobilizerResult;

        public ResultCombiner(NewsItem newsItem, MobilizerResult mobilizerResult)
        {
            if (newsItem == null) throw new ArgumentNullException("newsItem");
            if (mobilizerResult == null) throw new ArgumentNullException("mobilizerResult");

            this.newsItem = newsItem;
            this.mobilizerResult = mobilizerResult;
        }

        public MobilizedArticle Combine()
        {
            var heroImage = SelectBestImage();

            var fullDate =
                newsItem.LocalDateTime.ToString("dddd, MMMM dd • h:mm") +
                newsItem.LocalDateTime.ToString("tt").ToLowerInvariant();

            return new MobilizedArticle
            {
                Title = newsItem.Title,
                HeroImageUrl = heroImage,
                Link = newsItem.Link,
                Source = newsItem.OriginalSource,
                FullDate = fullDate,
                CombinedPublicationAndDate = newsItem.FormattedForMainPageSourceAndDate,
                ContentHtml = mobilizerResult.content,
                Author = mobilizerResult.author,
            };
        }

        string SelectBestImage()
        {
            if (!string.IsNullOrWhiteSpace(mobilizerResult.lead_image_url))
                return mobilizerResult.lead_image_url;

            if (newsItem.HasImage)
                return newsItem.HighestQualityImageUrl;

            return null;
        }
    }
}
