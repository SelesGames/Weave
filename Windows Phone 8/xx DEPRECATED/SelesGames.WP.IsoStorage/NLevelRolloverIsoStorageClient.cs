using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;

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

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var fileName in fileNames)
                {
                    var tryResult = await TryGetAsync(store, fileName, cancelToken).ConfigureAwait(false);
                    if (tryResult.IsSuccess)
                    {
                        foundResult = true;
                        result = tryResult.Result;
                        break;
                    }
                    else
                        aggregateExceptions.Add(tryResult.Exception);
                }
            }

            if (foundResult)
                return result;
            else
                throw new AggregateException(aggregateExceptions);
        }

        async Task<TryResult> TryGetAsync(IsolatedStorageFile file, string fileName, CancellationToken cancelToken)
        {
            try
            {
                var result = await storageClient.GetAsync(file, fileName, cancelToken).ConfigureAwait(false);
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
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                await storageClient.SaveAsync(TempFileName, obj, cancelToken).ConfigureAwait(false);

                for (int i = fileNames.Length - 1; i > 0; i--)
                {
                    int j = i - 1;

                    var sourceFile = fileNames[j];
                    var destinationFile = fileNames[i];

                    if (store.FileExists(sourceFile))
                    {
                        if (store.FileExists(destinationFile))
                            store.DeleteFile(destinationFile);

                        store.MoveFile(sourceFile, destinationFile);
                    }
                }

                store.MoveFile(TempFileName, fileNames[0]);
            }
        }

        #endregion
    }
}
