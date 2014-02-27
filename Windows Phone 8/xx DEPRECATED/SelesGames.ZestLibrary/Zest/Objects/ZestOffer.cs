using System.Collections.Generic;

namespace ZuneCrawler.Core.Zest
{
    internal class ZestOffer
    {
        public string OfferId { get; set; }
        public string MediaInstanceId { get; set; }
        public decimal Price { get; set; }
        public string PriceCurrencyCode { get; set; }
        public string LicenseRight { get; set; }
        public List<string> PaymentType = new List<string>();
    }
}
