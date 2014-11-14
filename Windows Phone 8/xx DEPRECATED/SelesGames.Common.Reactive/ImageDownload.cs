using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SelesGames.Common.Reactive
{
    public class ImageDownload : IDisposable
    {
        readonly string imageUrl;
        readonly Dispatcher dispatcher;
        Task<BitmapImage> imageAwaiter;

        bool isDisposed = false;

        public ImageDownload(string imageUrl, Dispatcher dispatcher)
        {
            this.imageUrl = imageUrl;
            this.dispatcher = dispatcher;
            BeginImageDownload();
        }

        public Task<BitmapImage> GetImage()
        {
            return imageAwaiter;
        }

        async void BeginImageDownload()
        {
            imageAwaiter = GetImageInnerImp();

            try
            {
                await imageAwaiter;
            }
            catch { }

            if (isDisposed)
                DestroyBitmap();
        }

        async Task<BitmapImage> GetImageInnerImp()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(imageUrl);

            if (isDisposed)
                throw new ObjectDisposedException(this.ToString());

            response.EnsureSuccessStatusCode();

            using (var imageStream = await response.Content.ReadAsStreamAsync())
            {
                if (isDisposed)
                    throw new ObjectDisposedException(this.ToString());

                var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.IgnoreImageCache | BitmapCreateOptions.BackgroundCreation };
                bmp.SetSource(imageStream);
                return bmp;
            }
        }

        void DestroyBitmap()
        {
            dispatcher.BeginInvoke(() =>
            {
                if (imageAwaiter.Status != TaskStatus.RanToCompletion)
                    return;

                var image = imageAwaiter.Result;

                if (image != null)
                {
                    image.UriSource = null;
                    image = null;
                }
            });
        }

        public void Dispose()
        {
            isDisposed = true;
            DestroyBitmap();
        }
    }
}
