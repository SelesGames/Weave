using System.Windows.Media;

namespace weave
{
    public class PhoneAccentComplementaryColorPalette
    {
        public SolidColorBrush AccentBrush { get; set; }
        public SolidColorBrush AccentDullBrush { get; set; }
        public SolidColorBrush AccentBrightBrush { get; set; }
        public SolidColorBrush ComplementaryBrush { get; set; }
        public SolidColorBrush ComplementaryDullBrush { get; set; }

        public string RGBAccentBrush { get; set; }
        public string RGBComplementaryBrush { get; set; }

        public static PhoneAccentComplementaryColorPalette CreateFromBaseColor(Color color)
        {
            var complementary = color.GetComplementary();
            var palette = new PhoneAccentComplementaryColorPalette
            {
                AccentBrush = new SolidColorBrush(color),
                AccentDullBrush = new SolidColorBrush(complementary.OriginalColorDarker),
                AccentBrightBrush = new SolidColorBrush(complementary.OriginalColorLighter),
                ComplementaryBrush = new SolidColorBrush(complementary.ComplementaryColorLighter),
                ComplementaryDullBrush = new SolidColorBrush(complementary.ComplementaryColorDarker),
            };

            palette.RGBAccentBrush = GetRGBHexFromColor(palette.AccentBrush.Color);
            palette.RGBComplementaryBrush = GetRGBHexFromColor(palette.ComplementaryBrush.Color);

            return palette;
        }

        static string GetRGBHexFromColor(Color color)
        {
            return color.ToString().Substring(3).Insert(0, "#");
        }
    }
}
