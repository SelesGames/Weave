using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SelesGames.WP.PublisherInfoPage
{
    internal class RatingsImageBridge
    {
        public static readonly ImageSource zeroStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/0.png");
        public static readonly ImageSource oneHalfStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/0.5.png");
        public static readonly ImageSource oneStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/1.png");
        public static readonly ImageSource oneAndHalfStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/1.5.png");
        public static readonly ImageSource twoStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/2.png");
        public static readonly ImageSource twoAndHalfStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/2.5.png");
        public static readonly ImageSource threeStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/3.png");
        public static readonly ImageSource threeAndHalfStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/3.5.png");
        public static readonly ImageSource fourStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/4.png");
        public static readonly ImageSource fourAndHalfStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/4.5.png");
        public static readonly ImageSource fiveStar = MakeBitmap("/SelesGames.WP.PublisherInfoPage;component/Assets/Stars/5.png");

        static ImageSource MakeBitmap(string p)
        {
            return new BitmapImage { UriSource = new Uri(p, UriKind.Relative), CreateOptions = BitmapCreateOptions.None };
        }
    }

    internal static class RatingsImageBridgeExtensions
    {
        public static ImageSource GetRatingImageFromDecimal(this decimal rating)
        {
            if (rating > 9.5m)
                return RatingsImageBridge.fiveStar;

            else if (rating > 8.5m)
                return RatingsImageBridge.fourAndHalfStar;

            else if (rating > 7.5m)
                return RatingsImageBridge.fourStar;

            else if (rating > 6.5m)
                return RatingsImageBridge.threeAndHalfStar;

            else if (rating > 5.5m)
                return RatingsImageBridge.threeStar;

            else if (rating > 4.5m)
                return RatingsImageBridge.twoAndHalfStar;

            else if (rating > 3.5m)
                return RatingsImageBridge.twoStar;

            else if (rating > 2.5m)
                return RatingsImageBridge.oneAndHalfStar;

            else if (rating > 1.5m)
                return RatingsImageBridge.oneStar;

            else // (Rating >= 0.5m
                return RatingsImageBridge.oneHalfStar;
        }
    }
}
