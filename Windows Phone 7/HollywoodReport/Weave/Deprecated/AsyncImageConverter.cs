using System;
using System.Windows.Data;
using System.Windows.Media;

namespace weave
{
    public class AsyncImageSourceConverter : IValueConverter
    {
        ImageCache backingCache = new ImageCache();

        public void Flush()
        {
            backingCache.Flush();
        }

        ImageSource GetImage(string url)
        {
            return backingCache.GetImage(url);
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string && targetType == typeof(ImageSource)))         
                return null;

            var url = (string)value;

            return GetImage(url);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
