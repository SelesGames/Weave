
namespace SelesGames.UI.Advertising
{
    public abstract class AdSettingsBase
    {
        public bool Enabled { get; set; }
        public int FaultToleranceCount { get; set; }
        public int DisplayTime { get; set; }

        public AdSettingsBase()
        {
            Enabled = true;
            FaultToleranceCount = 1;
            DisplayTime = 30;
        }

        public abstract IAdControlAdapter CreateAdControl(string keywords);
    }
}
