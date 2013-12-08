using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Windows;

namespace Weave.Settings
{
    public class AppSettings
    {
        public AppSettings()
        {
            NumberOfNewsItemsPerMainPage = 16;
        }

        public string AppName { get; set; }
        public string VersionNumber { get; set; }
        public bool IsTrial { get; set; }
        public int NumberOfNewsItemsPerMainPage { get; set; }
        public bool IsAddSupportedApp { get; set; }
        public string AdUnitsUrl { get; set; }
        public string AssemblyName { get; set; }
        public bool IsNetworkAvailable { get; set; }
        public bool LogExceptions { get; set; }
        public List<string> AdUnits { get; set; }
        public StandardThemeSet Themes { get; set; }
        public Application CurrentApplication { get; set; }
        public StartupMode StartupMode { get; set; }
        public string ExpandedFeedLibraryUrl { get; set; }

        public static AppSettings Instance { get; set; }
    }
}
