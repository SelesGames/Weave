using System.Windows.Controls;

namespace SelesGames.UI.Advertising
{
    public static class AdControlExtensionMethods
    {
        public static SwitchingAdControl AddAdControlIfRelevant(this Panel panel, AdControlFactory factory, string keywords = null, bool playAnimations = false)
        {
            if (!AdVisibilityService.AreAdsStillBeingShownAtAll)
                return null;

            var adControl = new SwitchingAdControl(factory, keywords) { PlayAnimations = playAnimations };

            panel.Children.Add(adControl);
            return adControl;
        }
    }
}
