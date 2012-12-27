
namespace SelesGames.UI.Advertising
{
    public static class AdSettings
    {
        public static bool IsAddSupportedApp { get; set; }

        static int maxAdsPerSession = 1;

        public static int MaxAdsPerSession
        {
            get { return maxAdsPerSession; }
            set { maxAdsPerSession = value; }
        }
    }
}
