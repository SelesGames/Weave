using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SelesGames.Common.Reactive
{
    public class ImageDownload2 : IDisposable
    {
        readonly string imageUrl;
        readonly Dispatcher dispatcher;
        readonly List<BitmapImage> realizedBitmaps;
        Task<Stream> imageAwaiter;

        bool isDisposed = false;

        public ImageDownload2(string imageUrl, Dispatcher dispatcher)
        {
            this.imageUrl = imageUrl;
            this.dispatcher = dispatcher;
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
            imageAwaiter = GetImageStream();

            try
            {
                await imageAwaiter;
            }
            catch { }

            if (isDisposed)
                Teardown();
        }

        async Task<Stream> GetImageStream()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(imageUrl);

            if (isDisposed)
                throw new ObjectDisposedException(this.ToString());

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            if (isDisposed)
                throw new ObjectDisposedException(this.ToString());

            return stream;
        }

        void Teardown()
        {
            dispatcher.BeginInvoke(() =>
            {
                if (imageAwaiter.Status != TaskStatus.RanToCompletion)
                    return;

                var imageStream = imageAwaiter.Result;

                if (imageStream != null)
                    imageStream.Dispose();

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
