using System.Globalization;

namespace System
{
    public static class DateTimeExtensions
    {
        public static DateTime AsDateTimeOrDefault(this string val, DateTime defaultVal)
        {
            DateTime temp;
            if (DateTime.TryParse(val, null, DateTimeStyles.AdjustToUniversal, out temp))
                return temp.ToLocalTime();
            else
                return defaultVal.ToLocalTime();
        }
    }
}
