using System.Windows.Media;

namespace weave
{
    public class FontProperties
    {
        public string FontName { get; private set; }
        public FontFamily FontFamily { get; private set; }

        public FontProperties(string fontName)
        {
            FontName = fontName;
            FontFamily = new FontFamily(fontName);
        }
    }
}
