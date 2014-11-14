using SelesGames.ZestLibrary;
using System.Windows.Media;

namespace SelesGames.WP.PublisherInfoPage
{
    public class ZuneAppViewModel
    {
        public string AppId { get; set; }
        public string AppName { get; set; }
        public ImageSource Image { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public int UserRatingCount { get; set; }
        public string OfferType { get; set; }
        public ImageSource BackgroundImage { get; set; }

        public string FormattedPrice
        {
            get
            {
                if (OfferType == "Free")
                    return "Free";

                else if (OfferType == "Paid")
                    return string.Format("$ {0}", Price);

                else if (OfferType == "Trial")
                    return string.Format("$ {0} / Trial", Price);

                else return null;
            }
        }

        public ImageSource RatingStars
        {
            get
            {
                if (UserRatingCount > 0)
                    return Rating.GetRatingImageFromDecimal();
                else
                    return RatingsImageBridge.zeroStar;
            }
        }

        public string MediumImageUrl
        {
            get { return ZuneImageService.CreateMediumImageUrlFromAppId(AppId); }
        }

        //public Task LoadAppIconAsync()
        //{
        //    var t = new TaskCompletionSource<Unit>();

        //    var image = new BitmapImage { CreateOptions = BitmapCreateOptions.BackgroundCreation };

        //    image.ImageOpenedOrFailed().ToTask().ContinueWith(o =>
        //    {
        //        if (o.Exception != null)
        //            t.SetException(o.Exception.Flatten());
        //        else
        //            t.SetResult(Unit.Default);
        //    });

        //    Image = image;
        //    image.UriSource = new Uri(MediumImageUrl);

        //    return t.Task;
        //}
    }
}
