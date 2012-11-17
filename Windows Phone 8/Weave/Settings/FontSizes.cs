using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Weave.Customizability;

namespace weave
{
    public class FontSizes : List<FontSizeProperties>
    {
        public FontSizes()
        {
            /*
                *  Description =       0.95    *   Title
                *  PublicationLine =   0.80    *   Title
                *  LineHeight =        1.5     *   Description
            */
            this.Add(new FontSizeProperties
            {
                Id = FontSize.Small,                    // PhoneFontSizeNormal
                DisplayName = "small",
                TitleSize = 20d,                // 15 pt
                DescriptionSize = 19d,          // 14.25 pt
                LineHeight = 29.333d,           // 22 pt
                PublicationLineSize = 16d,      // 12 pt
                MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 9d),       // PERFECT
            });


            this.Add(new FontSizeProperties
            {
                Id = FontSize.Medium,                   // PhoneFontSizeMedium
                DisplayName = "medium",
                TitleSize = 22.667d,            // 17 pt
                DescriptionSize = 21.333d,      // 16 pt
                LineHeight = 32d,               // 24 pt
                PublicationLineSize = 18d,      // 13.5 pt
                //MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 24d),       // PERFECT
                MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 24d),       // PERFECT
            });


            this.Add(new FontSizeProperties
            {
                Id = FontSize.MediumLarge,              // PhoneFontSizeMediumLarge
                DisplayName = "medium large",
                TitleSize = 25.333d,            // 19 pt
                DescriptionSize = 24d,          // 18 pt
                LineHeight = 36d,               // 27 pt
                PublicationLineSize = 20d,      // 15 pt
                //MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 33d)
                MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 33d)
            });


            this.Add(new FontSizeProperties
            {
                Id = FontSize.Large,                    // PhoneFontSizeLarge
                DisplayName = "large",
                TitleSize = 32d,                // 24 pt
                DescriptionSize = 30d,          // 22.5 pt
                LineHeight = 44d,               // 33 pt
                PublicationLineSize = 25.333d,  // 19 pt
                //MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 42d),
                MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 33d),
            });


            this.Add(new FontSizeProperties
            {
                Id = FontSize.ExtraLarge,               // almost PhoneFontSizeExtraLarge
                DisplayName = "extra large",
                TitleSize = 40d,                // 30 pt
                DescriptionSize = 37.333d,      // 28 pt
                LineHeight = 56d,               // 42 pt
                PublicationLineSize = 32d,      // 24 pt
                //MainPageNewsItemMargin = new Thickness(0d, 3d, 0d, 93d),
                //MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 51d),
                MainPageNewsItemMargin = new Thickness(0d, 12d, 0d, 42d),
            });
        }

        public FontSizeProperties GetById(FontSize id)
        {
            var matching = this.SingleOrDefault(o => o.Id == id);
            return matching;
        }
    }
}
