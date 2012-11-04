using System.Collections.Generic;
using System.Linq;

namespace weave
{
    public class StandardFontSet : List<FontProperties>
    {
        public StandardFontSet()
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

        public FontProperties GetByFontName(string fontName)
        {
            var matching = this.SingleOrDefault(o => o.FontName == fontName);
            return matching;
        }
    }
}
