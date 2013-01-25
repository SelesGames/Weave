using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.Storage;

namespace Weave.LiveTile.ScheduledAgent
{
    public class LockScreenSavingClient
    {
        static string FILE_PATH_SCHEMA = "ms-appdata:///Local/LockScreen/";
        static string TILES_FOLDER = "LockScreen";
        //static string SHARED_SHELL_CONTENT_DIR = "/Shared/ShellContent/";
        string isoStorageFullFileName;
        string lockScreenFileName;
        MemoryStream ms;
        Uri fullAppDataFileUri;

        public async Task<Tuple<bool, Uri>> TryGetLocalStorageUri(Uri isoStorageFullFileName)
        {
            this.isoStorageFullFileName = isoStorageFullFileName.OriginalString;
            ms = new MemoryStream();

            try
            {
                var canRead = await ReadFromIsoStorage();
                if (!canRead)
                    return Tuple.Create(false, default(Uri));

                DetermineFileName();
                await SaveToLocalStorage();

                var path = string.Format("{0}{1}", FILE_PATH_SCHEMA, lockScreenFileName);
                fullAppDataFileUri = new Uri(path, UriKind.Absolute);

                return Tuple.Create(true, fullAppDataFileUri);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                return Tuple.Create(false, default(Uri));
            }
        }

        async Task SaveToLocalStorage()
        {
            StorageFolder tilesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(TILES_FOLDER, CreationCollisionOption.OpenIfExists);

            foreach (var existingFile in await tilesFolder.GetFilesAsync())
                await existingFile.DeleteAsync();

            var file = await tilesFolder.CreateFileAsync(lockScreenFileName, CreationCollisionOption.ReplaceExisting);

            using (StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync())
            using (var stream = transaction.Stream.AsStreamForWrite())
            {
                await ms.CopyToAsync(stream);
                await transaction.CommitAsync();
            }
        }




        #region private helper functions

        public async Task<bool> ReadFromIsoStorage()
        {
            var isoFileName = isoStorageFullFileName.Replace("isostore:", "");

            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.FileExists(isoFileName))
                    return false;

                using (var stream = file.OpenFile(isoFileName, FileMode.Open))
                {
                    await stream.CopyToAsync(ms);
                    ms.Position = 0;
                    return true;
                }
            }
        }

        void DetermineFileName()
        {
            //var currentImage = LockScreen.GetImageUri();

            //if (currentImage.ToString().EndsWith("_A.jpg"))
            //    lockScreenFileName = "LiveLockBackground_B.jpg";

            //else
            //    lockScreenFileName = "LiveLockBackground_A.jpg";
            lockScreenFileName = Guid.NewGuid().ToString() + ".jpg";
        }

        #endregion
    }
}
