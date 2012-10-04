using System.Windows.Controls;

namespace weave.UI.Advertising
{
    public static class AdControlExtensionMethods
    {
        public static TrialModeAdControl AddAdControlIfRelevant(this Panel panel, bool playAnimations = false)
        {
            if (!AdVisibilityService.AreAdsStillBeingShownAtAll)
                return null;

            var adControl = new TrialModeAdControl { PlayAnimations = playAnimations };

            panel.Children.Add(adControl);
            return adControl;
        }
    }
}
