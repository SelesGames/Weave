using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using System;
using Microsoft.Phone.Scheduler;
using System.Linq;

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

            var settings = new AppSettings
            {
                AppName = "Weave",
                VersionNumber = "4.0",
                IsTrial = new Microsoft.Phone.Marketplace.LicenseInformation().IsTrial(),
                CanSelectInitialCategories = true,
                AssemblyName = "weave",
                LogExceptions = true,

                #region Ad Units

                IsAddSupportedApp = true,
                AdApplicationId = "7e2d7892-037b-422b-bf91-7931d602864b",
                AdUnits = new List<string> { "31892" },

                #endregion

                CurrentApplication = this,
                ExpandedFeedLibraryUrl = "http://weavestorage.blob.core.windows.net/settings/masterfeeds_compressed.xml?xsf=" + new System.Random().Next(123, 978),
            };

            new WeaveStartupTask(settings);

            EnableLiveTile();
        }

        string agentName = "AllNewsAgent";




        #region Programatically Enable Live Tiles

        void EnableLiveTile()
        {
            // Obtain a reference to the period task, if one exists
            var periodicTask = ScheduledActionService.Find(agentName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            try
            {
                if (periodicTask != null)
                    ScheduledActionService.Remove(agentName);

                var actions = ScheduledActionService.GetActions<PeriodicTask>().ToList();
                foreach (var action in actions)
                    ScheduledActionService.Remove(action.Name);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }


            periodicTask = new PeriodicTask(agentName)
            {
                Description = "Enables *** LIVE TILE *** updating for Weave.  If you disable this, you will lose Live Tiles for this app."
            };

            // Place the call to Add in a try block in case the user has disabled agents
            try
            {
                ScheduledActionService.Add(periodicTask);
#if DEBUG
                //if (System.Diagnostics.Debugger.IsAttached)
                //    ScheduledActionService.LaunchForTest(agentName, TimeSpan.FromSeconds(40));
#endif
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    //MessageBox.Show("Background agents for this application have been disabled by the user.");
                }
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
            }
            catch (SchedulerServiceException)
            {
                // No user action required.
            }
        }

        #endregion

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
