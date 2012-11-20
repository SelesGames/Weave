using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SelesGames.IsoStorage
{
    public class NLevelRolloverIsoStorageClient<T>
    {
        IsoStorageClient<T> storageClient;
        string[] fileNames;

        public string TempFileName { get; set; }

        public NLevelRolloverIsoStorageClient(IsoStorageClient<T> storageClient, params string[] fileNames)
        {
            if (fileNames.Length <= 0)
                throw new Exception("must have some filenames!!");

            this.storageClient = storageClient;
            this.fileNames = fileNames;

            TempFileName = Guid.NewGuid().ToString();
        }




        #region Get

        public async Task<T> GetAsync(CancellationToken cancelToken)
        {
            T result = default(T);
            bool foundResult = false;
            List<Exception> aggregateExceptions = new List<Exception>();

            var folder = ApplicationData.Current.LocalFolder;
            foreach (var fileName in fileNames)
            {
                var tryResult = await TryGetAsync(folder, fileName, cancelToken).ConfigureAwait(false);
                if (tryResult.IsSuccess)
                {
                    foundResult = true;
                    result = tryResult.Result;
                    break;
                }
                else
                    aggregateExceptions.Add(tryResult.Exception);
            }

            if (foundResult)
                return result;
            else
                throw new AggregateException(aggregateExceptions);
        }

        async Task<TryResult> TryGetAsync(StorageFolder folder, string fileName, CancellationToken cancelToken)
        {
            try
            {
                var result = await storageClient.GetAsync(folder, fileName, cancelToken).ConfigureAwait(false);
                return new TryResult { IsSuccess = true, Result = result };
            }
            catch(Exception e)
            {
                return new TryResult { IsSuccess = false, Exception = e };
            }
        }

        class TryResult
        {
            internal T Result { get; set; }
            internal bool IsSuccess { get; set; }
            internal Exception Exception { get; set; }
        }

        #endregion




        #region Save

        public async Task SaveAsync(T obj, CancellationToken cancelToken)
        {
            var folder = ApplicationData.Current.LocalFolder;

            await storageClient.SaveAsync(TempFileName, obj, cancelToken).ConfigureAwait(false);

            for (int i = fileNames.Length - 1; i > 0; i--)
            {
                int j = i - 1;

                var sourceFile = fileNames[j];
                var destinationFile = fileNames[i];

                await folder.TryRenameFileAsync(sourceFile, destinationFile).ConfigureAwait(false);
            }

            await folder.TryRenameFileAsync(TempFileName, fileNames[0]);
        }

        #endregion
    }

    internal static class StorageFolderExtensions
    {
        public static async Task<bool> TryRenameFileAsync(this StorageFolder folder, string sourceFileName, string destinationFileName)
        {
            bool isSuccess = false;

            try
            {
                var fileToRename = await folder.GetFileAsync(sourceFileName);
                await fileToRename.RenameAsync(destinationFileName);
                isSuccess = true;
            }
            catch { }
            return isSuccess;
        }
    }
}
