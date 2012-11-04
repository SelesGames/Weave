using System.Windows;

namespace weave
{
    public class FontSizeProperties
    {
        public FontSize Id { get; set; }
        public string DisplayName { get; set; }
        public double TitleSize { get; set; }
        public double DescriptionSize { get; set; }
        public double PublicationLineSize { get; set; }
        public double LineHeight { get; set; }
        public Thickness MainPageNewsItemMargin { get; set; }

        public string HtmlTextSize()
        {
            return PixelsToPoints(TitleSize).ToString() + "px";
        }

        //public static double PointsToPixels(double points)
        //{
        //    return points * (96.0 / 72.0);
        //}

        public static double PixelsToPoints(double pixels)
        {
            //return pixels * (72.0 / 96.0);
            return pixels * (2.0 / 3.0);
        }
    }
}
