using System;
using System.Net;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace weave
{
    public class ImageBrushConverter : IValueConverter
    {
        ImageCache imageCache = new ImageCache();

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string source = (string)value;
                var uri = source.ToUri();
                if (uri == null)
                    return null;

                var x = imageCache.GetImageWithNotification(source);
                return new ImageBrush { ImageSource = x.Item1 };

                //BitmapImage image = new BitmapImage(uri);
                //if (image == null)
                //    return null;

                //return new ImageBrush { ImageSource = image };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
