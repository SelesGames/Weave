using System.Windows.Media;

namespace Weave.Customizability
{
    public class ReadingTheme
    {
        Brush text, background, accent, complementary, subtle;

        public string Name { get; set; }

        public Brush TextBrush
        {
            get { return text; }
            set { text = value; Text = GetRGBHexFromBrush(value); }
        }

        public Brush BackgroundBrush
        {
            get { return background; }
            set { background = value; Background = GetRGBHexFromBrush(value); }
        }

        public Brush AccentBrush                                // for coloring NEW articles, as well as article links and blockquotes
        {
            get { return accent; }
            set { accent = value; Accent = GetRGBHexFromBrush(value); }
        }

        public Brush ComplementaryBrush                         // for coloring FAVORITE articles
        {
            get { return complementary; }
            set { complementary = value; Complementary = GetRGBHexFromBrush(value); }
        }

        public Brush SubtleBrush                                // for coloring OLD articles
        {
            get { return subtle; }
            set { subtle = value; Subtle = GetRGBHexFromBrush(value); }
        }

        public string Text { get; set; }
        public string Background { get; set; }
        public string Accent { get; set; }
        public string Complementary { get; set; }
        public string Subtle { get; set; }

        static string GetRGBHexFromBrush(Brush brush)
        {
            if (!(brush is SolidColorBrush))
                return null;

            var sb = (SolidColorBrush)brush;
            if (sb.Color == null)
                return null;

            return sb.Color.ToString().Substring(3).Insert(0, "#");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
