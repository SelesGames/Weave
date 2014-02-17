
namespace SelesGames.UI.Advertising
{
    public abstract class AdProviderSettingsBase
    {
        public bool Enabled { get; set; }
        public int FaultToleranceCount { get; set; }
        public int DisplayTime { get; set; }

        public AdProviderSettingsBase()
        {
            Enabled = true;
            FaultToleranceCount = 1;
            DisplayTime = 30;
        }

        public abstract IAdControlAdapter CreateAdControl(string keywords);
    }
}
