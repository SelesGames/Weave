using Microsoft.Phone.Shell;
using SelesGames.IsoStorage;
using System;
using System.Collections.Generic;
using System.Windows;
using weave.Services.MostViewedHistory;

namespace weave
{
    public class AppSettings
    {
        const string TOMBSTONE_STATE = "tmbstt";
        const string PERMA_STATE = "permstt";

        public AppSettings()
        {
            NumberOfNewsItemsPerMainPage = 16;

            PermanentState = new IsoStorageLocker<PermanentState>(
                PERMA_STATE,
                new JsonIsoStorageClient<PermanentState>(new[] { typeof(RunLog), typeof(LabelTally) }),
                () => new PermanentState());

            TombstoneState = new IsoStorageLocker<TombstoneState>(
                TOMBSTONE_STATE,
                new JsonIsoStorageClient<TombstoneState>(new[] { typeof(ReadabilityPageViewModel), typeof(NewsItem) }),
                () => new TombstoneState());
        }

        public string AppName { get; set; }
        public string VersionNumber { get; set; }
        public bool IsTrial { get; set; }
        public bool CanSelectInitialCategories { get; set; }
        public int NumberOfNewsItemsPerMainPage { get; set; }
        public IsoStorageLocker<PermanentState> PermanentState { get; private set; }
        public IsoStorageLocker<TombstoneState> TombstoneState { get; private set; }
        public string AdApplicationId { get; set; }
        public bool IsAddSupportedApp { get; set; }
        public string AdUnitsUrl { get; set; }
        public string AssemblyName { get; set; }
        public bool IsNetworkAvailable { get; set; }
        public bool LogExceptions { get; set; }
        public List<string> AdUnits { get; set; }
        public StandardThemeSet Themes { get; set; }
        public Application CurrentApplication { get; set; }
        public StartupMode StartupMode { get; set; }
        public DateTime LastLoginTime { get; set; }
        public string ExpandedFeedLibraryUrl { get; set; }

        public static AppSettings Instance { get; set; }
    }
}
