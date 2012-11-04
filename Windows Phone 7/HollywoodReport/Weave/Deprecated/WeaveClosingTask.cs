using SelesGames;

namespace weave
{
    public class WeaveClosingTask
    {
        public WeaveClosingTask()
        {
            AppSettings.Instance.TombstoneState.Save();
            AppSettings.Instance.PermanentState.Save();

            var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
            dal.SaveOnExit();
        }
    }
}
