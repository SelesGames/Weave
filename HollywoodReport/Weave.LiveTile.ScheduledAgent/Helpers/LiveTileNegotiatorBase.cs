using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;

namespace Weave.LiveTile.ScheduledAgent
{
    public abstract class LiveTileNegotiatorBase
    {
        protected LiveTileViewModel ViewModel { get; set; }
        ShellTile tile;

        public LiveTileNegotiatorBase(ShellTile tile)
        {
            this.tile = tile;
        }

        protected async Task<ImageSource> GetImageAsync(string url)
        {
            var bitmap = new WriteableBitmap(0, 0);

            var request = CreateResizerRequest(url);
            request.AllowReadStreamBuffering = true;
            var response = await request.GetResponseAsync();
            using (var stream = response.GetResponseStream())
            {
                bitmap.SetSource(stream);
                stream.Close();
            }
            return bitmap;
        }

        HttpWebRequest CreateResizerRequest(string imageUrl)
        {
            var url = string.Format("http://sg-imaging.cloudapp.net/api/ImageResizer?quality=50&size=200&imageUrl={0}", HttpUtility.UrlEncode(imageUrl));
            var request = HttpWebRequest.CreateHttp(url);
            return request;
        }

        protected abstract Task InitializeViewModelAsync();

        public async Task UpdateTile()
        {
            if (tile == null) return;

            await InitializeViewModelAsync();

            if (ViewModel == null) return;

            var frontTile = new LiveTileFront();
            var backTile = new LiveTileBack();

            frontTile.DataContext = ViewModel;
            backTile.DataContext = ViewModel;

            var frontUri = frontTile.GetLiveTileUri("front");
            var backUri = backTile.GetLiveTileUri("back");

            var newTileData = new StandardTileData
            {
                BackgroundImage = frontUri,
                BackBackgroundImage = backUri,
                BackContent = string.Empty,
                BackTitle = string.Empty,
                Count = null,
                Title = string.Empty,
            };

            tile.Update(newTileData);
        }
    }
}
