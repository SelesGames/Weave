using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SelesGames.Common.Reactive
{
    public class ImageDownload3 : IDisposable
    {
        readonly string imageUrl;
        readonly Dispatcher dispatcher;
        readonly ImageStreamCache cache;
        readonly List<BitmapImage> realizedBitmaps;
        Task<Stream> imageAwaiter;

        bool isDisposed = false;

        public ImageDownload3(string imageUrl, Dispatcher dispatcher, ImageStreamCache cache)
        {
            this.imageUrl = imageUrl;
            this.dispatcher = dispatcher;
            this.cache = cache;
            this.realizedBitmaps = new List<BitmapImage>();
            BeginImageDownload();
        }

        public async Task<BitmapImage> GetImage()
        {
            var imageStream = await imageAwaiter;

            var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.IgnoreImageCache | BitmapCreateOptions.BackgroundCreation };
            bmp.SetSource(imageStream);
            realizedBitmaps.Add(bmp);
            return bmp;
        }

        async void BeginImageDownload()
        {
            imageAwaiter = cache.Get(imageUrl);

            try
            {
                await imageAwaiter;
            }
            catch { }

            if (isDisposed)
                Teardown();
        }

        void Teardown()
        {
            dispatcher.BeginInvoke(() =>
            {
                if (imageAwaiter.Status != TaskStatus.RanToCompletion)
                    return;

                foreach (var image in realizedBitmaps)
                {
                    image.UriSource = null;
                }
                realizedBitmaps.Clear();
            });
        }

        public void Dispose()
        {
            isDisposed = true;
            Teardown();
        }
    }
}
