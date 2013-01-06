
namespace SelesGames.UI.Advertising
{
    public abstract class AdSettingsBase
    {
        public bool Enabled { get; set; }
        public int ExecutionOrder { get; set; }
        public int FaultToleranceCount { get; set; }

        public AdSettingsBase()
        {
            Enabled = true;
            FaultToleranceCount = 1;
        }

        public abstract IAdControlAdapter CreateAdControl(string keywords);
    }
}
