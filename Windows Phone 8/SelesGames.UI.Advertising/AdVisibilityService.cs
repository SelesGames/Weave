using System;

namespace SelesGames.UI.Advertising
{
    public static class AdVisibilityService
    {
        public static event EventHandler AdsNoLongerShown;

        static int numberOfTimesAdsShown = 0;

        public static bool AreAdsStillBeingShownAtAll
        {
            get
            {
                if (!(AdSettings.IsAddSupportedApp))
                    return false;

                if (numberOfTimesAdsShown >= AdSettings.MaxAdsPerSession)
                    return false;

                return true;
            }
        }

        public static void AdEngaged()
        {
            numberOfTimesAdsShown++;
            if (numberOfTimesAdsShown >= AdSettings.MaxAdsPerSession)
                if (AdsNoLongerShown != null)
                    AdsNoLongerShown(null, EventArgs.Empty);
        }
    }
}
