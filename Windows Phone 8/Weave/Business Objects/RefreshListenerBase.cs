using SelesGames;
using System.Threading.Tasks;
using weave.Data;

namespace weave
{
    public abstract class RefreshListenerBase
    {
        protected Weave4DataAccessLayer DataAccessLayer = ServiceResolver.Get<Weave4DataAccessLayer>();

        public abstract Task GetRefreshed();
        public abstract int GetNewCount();
    }
}
