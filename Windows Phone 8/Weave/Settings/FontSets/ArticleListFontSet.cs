using Weave.Customizability;

namespace weave
{
    public class ArticleListFontSet : FontSet
    {
        public ArticleListFontSet()
        {
            this.AddRange(new[] 
            { 
                new FontProperties("Segoe WP Light"), 
                new FontProperties("Segoe WP SemiLight"), 
                new FontProperties("Segoe WP"), 
                new FontProperties("Segoe WP Semibold"), 
                new FontProperties("Segoe WP Black"), 
                new FontProperties("Georgia"), 
                new FontProperties("Arial"), 
                new FontProperties("Tahoma"), 
                new FontProperties("Verdana"), 
                new FontProperties("Calibri"), 
            });
        }
    }
}
