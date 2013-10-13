using System.Diagnostics;
using System.Windows;

namespace weave
{
    public partial class App : Application
    {
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            var r = new System.Random().Next(123, 978);

            var settings = new AppSettings
            {
                AppName = "Weave",
                VersionNumber = "8.5.0",
                IsTrial = false,//new Microsoft.Phone.Marketplace.LicenseInformation().IsTrial(),
                AssemblyName = "weave",
                LogExceptions = true,

                #region Ad Units

                IsAddSupportedApp = false,
                AdUnitsUrl = "http://weave.blob.core.windows.net/settings/weaveAdSettings.json?xsf=" + r,

                #endregion

                CurrentApplication = this,
                ExpandedFeedLibraryUrl = "http://weave.blob.core.windows.net/settings/masterfeeds.xml?xsf=" + r,
            };

            new WeaveStartupTask(settings);
        }

        // Code to execute on Unhandled Exceptions
        void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                DebugEx.WriteLine(e);
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }
    }
}