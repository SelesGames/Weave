using System.Windows;

namespace weave
{
    public partial class App : Application
    {
        public App()
        {
            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;
            //Host.Settings.EnableFrameRateCounter = true;
            //Host.Settings.EnableRedrawRegions = true;
            //Host.Settings.EnableCacheVisualization = true;
            
            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            //InitializePhoneApplication();

            var r = new System.Random().Next(123, 978);

            var settings = new AppSettings
            {
                AppName = "Weave",
                VersionNumber = "4.0",
                IsTrial = new Microsoft.Phone.Marketplace.LicenseInformation().IsTrial(),
                CanSelectInitialCategories = true,
                AssemblyName = "weave",
                LogExceptions = true,

                #region Ad Units

                IsAddSupportedApp = false,
                AdApplicationId = "7e2d7892-037b-422b-bf91-7931d602864b",
                AdUnitsUrl = "http://weavestorage.blob.core.windows.net/settings/adunits?xsf=" + r,

                #endregion

                CurrentApplication = this,
                ExpandedFeedLibraryUrl = "http://weave.blob.core.windows.net/settings/masterfeeds.xml?xsf=" + r,
            };

            new WeaveStartupTask(settings);
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
