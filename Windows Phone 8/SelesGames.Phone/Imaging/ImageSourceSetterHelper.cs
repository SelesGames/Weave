using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SelesGames.Phone.Imaging
{
    public class ImageSourceSetterHelper : IDisposable
    {
        readonly Image image;
        readonly string imageUrl;
        readonly Action beforeDownload;
        readonly Action afterDownload;

        BitmapImage bmp;
        MemoryStream ms;
        bool isDisposed;

        public ImageSourceSetterHelper(Image image, string imageUrl, Action beforeDownload, Action afterDownload)
        {
            this.image = image;
            this.imageUrl = imageUrl;
            this.beforeDownload = beforeDownload;
            this.afterDownload = afterDownload;

            BeginImageDownload();
        }

        async void BeginImageDownload()
        {
            try
            {
                await DownloadImageAsync();
            }
            catch { }
        }

        async Task DownloadImageAsync()
        {
            var client = new System.Net.Http.HttpClient();
            using (var response = await client.GetAsync(imageUrl).ConfigureAwait(false))
            {
                if (isDisposed) return;

                if (response.IsSuccessStatusCode)
                {
                    ms = new MemoryStream();
                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        if (isDisposed) return;

                        stream.CopyTo(ms);
                    }
                    ms.Position = 0;
    
                    GlobalDispatcher.Current.BeginInvoke(() =>
                    {
                        try
                        {
                            if (isDisposed) return;

                            bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.IgnoreImageCache | BitmapCreateOptions.BackgroundCreation };
                            //bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
                            bmp.SetSource(ms);

                            beforeDownload();
                            image.Source = bmp;
                            afterDownload();
                        }
                        catch { }
                    });
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            try
            {
                if (ms != null)
                    ms.Dispose();
                
                if (bmp != null)
                {
                    bmp.UriSource = null;
                    using (var tempMs = new MemoryStream())
                    {
                        bmp.SetSource(tempMs);
                    }
                }
            }
            catch{}
        }
    }
}