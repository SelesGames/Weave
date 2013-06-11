using System.Collections.Generic;
using System.Linq;
using Weave.Customizability;

namespace weave
{
    public class ArticleListFormats : List<ArticleListFormatProperties>
    {
        public ArticleListFormats()
        {
            this.AddRange(new[]
            {
                new ArticleListFormatProperties
                {
                    FormatType = ArticleListFormatType.Card,
                    DisplayName = "Cards",
                    Description = "Beautiful card view.  The recommended way to view news."
                },
                new ArticleListFormatProperties
                {
                    FormatType = ArticleListFormatType.Tiles,
                    DisplayName = "Tiles",
                    Description = "Tile view - same as what you see on the home screen under the \"latest news\" section.",
                },
                new ArticleListFormatProperties
                {
                    FormatType = ArticleListFormatType.BigImage,
                    DisplayName = "Big images",
                    Description = "Text on top, large image below.  Similar to Digg or Rockmelt on iOS.",
                }, 
                new ArticleListFormatProperties 
                { 
                    FormatType = ArticleListFormatType.SmallImage,
                    DisplayName = "\"Classic\" list",
                    Description = "Text on the left, image thumbnail on the right.  Visually similar to NewsRob on Android.",
                }, 
                new ArticleListFormatProperties
                {
                    FormatType = ArticleListFormatType.TextOnly,
                    DisplayName = "Text only",
                    Description = "No images.  When you care only for the headline, this is the way to go.",
                }
            });
        }

        public ArticleListFormatProperties GetArticleListFormatByFormatType(ArticleListFormatType formatType)
        {
            var matching = this.SingleOrDefault(o => o.FormatType == formatType);
            return matching;
        }
    }
}
