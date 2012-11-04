using System;

namespace weave.UI.Advertising
{
    public static class AdVisibilityService
    {
        public static event EventHandler AdsNoLongerShown;

        static int numberOfTimesAdsShown = 0;

        public static bool AreAdsStillBeingShownAtAll
        {
            get
            {
                if (!(AdSettings.IsAddSupportedApp && AreThereAdUnitsDefined()))
                    return false;

                if (numberOfTimesAdsShown >= AdSettings.MaxAdsPerSession)
                    return false;

                return true;
            }
        }

        static bool AreThereAdUnitsDefined()
        {
            return
                AdSettings.AdApplicationId != null/* &&
                AdSettings.AdUnits != null &&
                AdSettings.AdUnits.Count > 0*/;
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
