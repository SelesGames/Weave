using SelesGames.IsoStorage;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.SavedState.MostViewedHistory;

namespace Weave.LiveTile.ScheduledAgent.Storage
{
    public class DataStorageClient
    {
        const string PERMA_STATE = "permstt";
        const string TOMBSTONE_STATE = "tmbstt";




        #region PermanentState

        public Task<PermanentState> GetPermanentState()
        {
            return CreatePermStateClient()
                .GetOrDefaultAsync(PERMA_STATE, () => new PermanentState());
        }

        public Task Save(PermanentState permanentState)
        {
            return CreatePermStateClient()
                .SaveAsync(PERMA_STATE, permanentState);
        }

        #endregion




        #region Helper methods

        IsoStorageClient<PermanentState> CreatePermStateClient()
        {
            return new JsonIsoStorageClient<PermanentState>();
        }

        #endregion
    }
}
