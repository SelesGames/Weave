using System;
using System.Windows.Data;
using System.Windows.Media;

namespace weave
{
    public class PanoramaAsyncImageSourceConverter : IValueConverter
    {
        static ImageCache cache = new ImageCache();

        static ImageSource GetImage(string url)
        {
            return cache.GetImage(url);
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
