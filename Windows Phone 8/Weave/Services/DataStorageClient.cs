using SelesGames.IsoStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.SavedState.MostViewedHistory;

namespace weave.Services
{
    public class DataStorageClient
    {
        const string TOMBSTONE_STATE = "tmbstt";
        const string PERMA_STATE = "permstt";

        public Task<PermanentState> Get<T>() where T : PermanentState
        {
            var isoLocker = new IsoStorageLocker<PermanentState>(
                PERMA_STATE,
                new JsonIsoStorageClient<PermanentState>(new[] { typeof(RunLog), typeof(LabelTally) }),
                () => new PermanentState());

            return isoLocker.Get();
        }

        //public Task<TombstoneState> Get<T>() where T : TombstoneState
        //{
        //    var isoLocker = new IsoStorageLocker<TombstoneState>(
        //        TOMBSTONE_STATE,
        //        new JsonIsoStorageClient<TombstoneState>(new[] { typeof(ReadabilityPageViewModel), typeof(NewsItem) }),
        //        () => new TombstoneState());

        //    return isoLocker.Get();
        //}
    }
}
