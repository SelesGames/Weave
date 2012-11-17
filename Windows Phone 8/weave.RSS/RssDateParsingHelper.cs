using System;
using System.Globalization;
using System.Linq;

namespace weave.Services.RSS
{
    public static class RssDateParsingHelper
    {
        public static Tuple<bool, DateTime> TryGetLocalDate(string dateTimeString)
        {
            try
            {
                if (string.IsNullOrEmpty(dateTimeString))
                    return Tuple.Create(false, DateTime.MinValue);

                DateTime dateTime;

                var canRfcParse = SyndicationDateTimeUtility
                    .TryParseRfc822DateTime(dateTimeString, out dateTime);

                if (!canRfcParse)
                {
                    var canNormalParse = DateTime.TryParse(dateTimeString, out dateTime);
                    if (!canNormalParse)
                    {
                        string canParseAny = new[] { "ddd, dd MMM yyyy HH:mm:ss ZK", "yyyy-MM-ddTHH:mm:ssK" }
                            .FirstOrDefault(o => DateTime.TryParseExact(dateTimeString, o, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));

                        if (canParseAny == null)
                            return Tuple.Create(false, DateTime.MinValue);
                    }
                }

                if (dateTime.Kind == DateTimeKind.Unspecified || dateTime.Kind == DateTimeKind.Utc)
                {
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
                }

                if (dateTime > DateTime.Now)
                    dateTime = DateTime.Now;

                return Tuple.Create(true, dateTime);
            }
            catch (Exception)
            {
                return Tuple.Create(false, DateTime.MinValue);
            }
        }
    }
}
