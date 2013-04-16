using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Weave.ViewModels.Extensions
{
    internal static class DateTimeDisplayFormattingExtensions
    {
        internal static string ElapsedTime(this DateTime time)
        {
            DateTime relativeTo = DateTime.Now;

            if (time > DateTime.Today)
                return time.ToString("h:mmt").ToLower();

            else if (time > DateTime.Today - TimeSpan.FromDays(5))
                return CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)time.DayOfWeek].ToCharArray().Take(3).AsString()
                    + time.ToString(" h:mmt").ToLower();

            else
                return time.ToString("M/d h:mmt").ToLower();
        }
    }

    internal static class charArrayExtension
    {
        public static string AsString(this IEnumerable<char> characters)
        {
            return new string(characters.ToArray());
        }
    }
}
