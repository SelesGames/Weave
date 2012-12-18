using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.LiveTile.ScheduledAgent
{
    public class LockScreenSavingClient
    {
        static string FILE_PATH_SCHEMA = "ms-appdata:///Local/LockScreen/";
        static string TILES_FOLDER = "LockScreen";

        public async Task<Uri> SaveToIsoStorage(Stream readStream)
        {
            var fileName = Guid.NewGuid().ToString();

            StorageFolder tilesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(TILES_FOLDER, CreationCollisionOption.OpenIfExists);
            var file = await tilesFolder.CreateFileAsync(tempImageFileName, CreationCollisionOption.ReplaceExisting);

            using (StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync())
            using (var stream = transaction.Stream.AsStreamForWrite())
            {
                await readStream.CopyToAsync(stream);
                await transaction.CommitAsync();
            }

            var path = string.Format("{0}{1}", FILE_PATH_SCHEMA, tempImageFileName);

            var url = new Uri(path, UriKind.Absolute);
            return url;
        }
    }
}
