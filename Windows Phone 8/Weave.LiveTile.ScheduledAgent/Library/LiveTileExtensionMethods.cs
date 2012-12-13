using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class LiveTileExtensionsMethods
    {
        static string SHARED_SHELL_CONTENT_DIR = "/Shared/ShellContent";

        //public static Uri GetLiveTileUri(this FrameworkElement element, string tempImageFileName)
        //{
        //    element.Measure(new Size(173, 173));
        //    element.Arrange(new Rect(0, 0, 173, 173));
        //    var bmp = new WriteableBitmap(173, 173);
        //    bmp.Render(element, null);
        //    bmp.Invalidate();

        //    return bmp.SaveToIsoStorage(tempImageFileName);
        //}

        //public static Uri SaveToIsoStorage(this WriteableBitmap bmp, string tempImageFileName)
        //{
        //    var fullFileName = string.Format("{0}/{1}", SHARED_SHELL_CONTENT_DIR, tempImageFileName);

        //    using (var file = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        if (!file.DirectoryExists(SHARED_SHELL_CONTENT_DIR))
        //            file.CreateDirectory(SHARED_SHELL_CONTENT_DIR);

        //        var originalWidth = bmp.PixelWidth;
        //        var originalHeight = bmp.PixelHeight;

        //        var newWidth = 100d;
        //        var scale = newWidth / originalWidth;
        //        var newHeight = scale * originalHeight;

        //        using (var stream = file.OpenFile(fullFileName, FileMode.OpenOrCreate))
        //        {
        //            bmp.SaveJpeg(stream, (int)newWidth, (int)newHeight, 0, 100);
        //        }
        //    }

        //    var url = new Uri("isostore:" + fullFileName, UriKind.Absolute);
        //    return url;
        //}

        public static async Task<Uri> SaveToIsoStorage(this Stream readStream, string tempImageFileName)
        {
            var fullFileName = string.Format("{0}/{1}", SHARED_SHELL_CONTENT_DIR, tempImageFileName);

            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists(SHARED_SHELL_CONTENT_DIR))
                    file.CreateDirectory(SHARED_SHELL_CONTENT_DIR);

                using (var stream = file.OpenFile(fullFileName, FileMode.OpenOrCreate))
                {
                    await readStream.CopyToAsync(stream);
                }
            }

            var url = new Uri("isostore:" + fullFileName, UriKind.Absolute);
            return url;
        }
    }
}
