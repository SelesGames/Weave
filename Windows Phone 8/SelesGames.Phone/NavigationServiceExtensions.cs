using System.Windows.Navigation;

namespace SelesGames.Phone
{
    public static class NavigationServiceExtensions
    {
        public static void SafelyGoBackIfPossible(this NavigationService navService)
        {
            try
            {
                if (navService.CanGoBack)
                    navService.GoBack();
            }
            catch { }
        }
    }
}
