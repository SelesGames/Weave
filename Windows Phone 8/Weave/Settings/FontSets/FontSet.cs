using System.Collections.Generic;
using System.Linq;
using Weave.Customizability;

namespace weave
{
    public class FontSet : List<FontProperties>
    {
        public FontProperties GetByFontName(string fontName)
        {
            var matching = this.SingleOrDefault(o => o.FontName == fontName);
            return matching;
        }
    }
}
