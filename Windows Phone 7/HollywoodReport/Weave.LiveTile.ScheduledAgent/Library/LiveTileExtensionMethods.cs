﻿using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class LiveTileExtensionsMethods
    {
        static string SHARED_SHELL_CONTENT_DIR = "/Shared/ShellContent";

        public static Uri GetLiveTileUri(this FrameworkElement element, string tempImageFileName)
        {
            element.Measure(new Size(173, 173));
            element.Arrange(new Rect(0, 0, 173, 173));
            var bmp = new WriteableBitmap(173, 173);
            bmp.Render(element, null);
            bmp.Invalidate();

            var fullFileName = string.Format("{0}/{1}", SHARED_SHELL_CONTENT_DIR, tempImageFileName);

            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists(SHARED_SHELL_CONTENT_DIR))
                    file.CreateDirectory(SHARED_SHELL_CONTENT_DIR);


                using (var stream = file.OpenFile(fullFileName, FileMode.OpenOrCreate))
                {
                    bmp.SaveJpeg(stream, 173, 173, 0, 100);
                }
            }

            var url = new Uri("isostore:" + fullFileName, UriKind.Absolute);
            return url;
        }
    }
}
