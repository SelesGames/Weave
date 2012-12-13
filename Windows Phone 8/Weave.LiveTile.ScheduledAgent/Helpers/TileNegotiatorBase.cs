using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Weave.LiveTile.ScheduledAgent.ViewModels;

namespace Weave.LiveTile.ScheduledAgent
{
    public abstract class TileNegotiatorBase
    {
        protected ITileViewModel ViewModel { get; set; }
        ShellTile tile;
        string appName;

        public TileNegotiatorBase(string appName, ShellTile tile)
        {
            this.appName = appName;
            this.tile = tile;
        }

        //protected async Task<ImageSource> GetImageAsync(string url)
        //{
        //    var bitmap = new WriteableBitmap(0, 0);

        //    var request = CreateResizerRequest(url);
        //    request.AllowReadStreamBuffering = true;
        //    var response = await request.GetResponseAsync();
        //    using (var stream = response.GetResponseStream())
        //    {
        //        bitmap.SetSource(stream);
        //        stream.Close();
        //    }
        //    return bitmap;
        //}

        //HttpWebRequest CreateResizerRequest(string imageUrl)
        //{
        //    var url = string.Format("http://sg-imaging.cloudapp.net/api/ImageResizer?quality=50&size=200&imageUrl={0}", HttpUtility.UrlEncode(imageUrl));
        //    var request = HttpWebRequest.CreateHttp(url);
        //    return request;
        //}

        protected abstract Task InitializeViewModelAsync();

        public async Task UpdateTile()
        {
            if (tile == null) return;

            await InitializeViewModelAsync();

            if (ViewModel == null) return;

            ViewModel.AppName = appName;

            var newTileData = ViewModel.CreateTileData();
            tile.Update(newTileData);
        }
    }
}
