
namespace weave
{
    public static class TaskService
    {
        public static void ToWebBrowserTask(string Url)
        {
            GlobalNavigationService.ToInternetExplorer(Url);
        }
    }
}
