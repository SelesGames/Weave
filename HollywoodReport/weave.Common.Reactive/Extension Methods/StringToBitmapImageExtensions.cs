using System.Net;

namespace System.Windows.Media.Imaging
{
    public static class StringToBitmapImageExtensions
    {
        public static BitmapImage ToBitmapImage(this string s,
            UriKind uriKind = UriKind.Absolute,
            BitmapCreateOptions createOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.IgnoreImageCache)
        {
            try
            {
                var uri = s.ToUri(uriKind);
                return uri.ToBitmapImage();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static BitmapImage ToBitmapImage(this Uri uri,
            BitmapCreateOptions createOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.IgnoreImageCache)
        {
            try
            {
                if (uri == null || (!(uri.Scheme == "http" || uri.Scheme == "https")))
                    return null;

                var bitmapImage = new BitmapImage(uri) { CreateOptions = createOptions };
                return bitmapImage;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
