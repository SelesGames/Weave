using Microsoft.Phone.Shell;
using System;
using System.Linq;
using Windows.Phone.System.UserProfile;

namespace Weave.LiveTile.ScheduledAgent.ViewModels
{
    public class CycleTileViewModel : ITileViewModel
    {
        public string AppName { get; set; }
        public Uri[] ImageIsoStorageUris { get; set; }
        public int? NewCount { get; set; }
        public Uri SmallBackgroundImageUri { get; set; }
        public Uri RecommendedLockScreenImageUri { get; set; }

        public ShellTileData CreateTileData()
        {
            return new CycleTileData
            {
                CycleImages = ImageIsoStorageUris,
                Title = AppName,
                Count = NewCount,
                SmallBackgroundImage = SmallBackgroundImageUri,
            };
        }

        public void UpdateLockScreen()
        {
            if (RecommendedLockScreenImageUri != null && LockScreenManager.IsProvidedByCurrentApplication)
            {
                LockScreen.SetImageUri(RecommendedLockScreenImageUri);
            }
        }
    }
}
