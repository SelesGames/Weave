using System;
using System.Linq;

namespace ZuneCrawler.Core.Zest
{
    internal static class ZestAppDataExtensionMethods
    {
        public static AppInfo ToZuneAppInfo(this ZestAppData zest)
        {
            if (zest == null)
                throw new ArgumentNullException("zest shall not be null");

            var category = zest.Categories.Where(o => o.IsRoot == "True").Select(o => o.Title).SingleOrDefault();

            var publisher = zest.Publisher.Select(o => o.Name).FirstOrDefault();


            string offerType;
            decimal price;

            var trialOffer = zest.Offers.Where(o => o.LicenseRight == "Trial").FirstOrDefault();
            var purchaseOffer = zest.Offers.Where(o => o.LicenseRight == "Purchase").FirstOrDefault();

            if (trialOffer != null && purchaseOffer != null)
            {
                offerType = "Trial";
                price = purchaseOffer.Price;
            }
            else if (trialOffer == null && purchaseOffer != null)
            {
                if (purchaseOffer.Price == 0)
                {
                    offerType = "Free";
                    price = 0;
                }
                else
                {
                    offerType = "Paid";
                    price = purchaseOffer.Price;
                }
            }
            else
            {
                offerType = "unknown";
                price = 0;
            }


            bool isXboxLive = false;
            var firstTag = zest.Tags.FirstOrDefault();
            if (!string.IsNullOrEmpty(firstTag) && firstTag == "apptag.premium")
                isXboxLive = true;


            return new AppInfo
            {
                Title = zest.Title,
                Id = zest.Id,
                ReleaseDate = zest.ReleaseDate,
                Version = zest.Version,
                ShortDescription = zest.ShortDescription,
                AverageUserRating = zest.AverageUserRating,
                UserRatingCount = zest.UserRatingCount,
                ImageId = zest.ImageId,
                Category = category,
                Publisher = publisher,
                OfferType = offerType,
                Price = price,
                IsXboxLive = isXboxLive,
            };
        }
    }
}
