using Weave.Customizability;

namespace Weave.Settings
{
    public class ArticleFontSet : FontSet
    {
        public ArticleFontSet()
        {
            this.AddRange(new[] 
            { 
                new FontProperties("Segoe WP"), 
                new FontProperties("Segoe WP Semibold"), 
                new FontProperties("Segoe WP SemiLight"), 
                new FontProperties("Georgia"), 
                new FontProperties("Arial"), 
                //new FontProperties("Malgun Gothic"), 
                //new FontProperties("Meiryo UI"),
                new FontProperties("Tahoma"), 
                new FontProperties("Verdana"), 
                new FontProperties("Calibri"), 
                //new FontProperties("Trebuchet MS"), 
            });
        }
    }
}
