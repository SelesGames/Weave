using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Weave.LiveTile.ScheduledAgent.Storage;
using Weave.SavedState;

namespace Weave.LiveTile.ScheduledAgent
{
    public class LiveTileUpdatingAgent : ScheduledTaskAgent
    {
        //static volatile bool isClassInitialized;

        string appName;


        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public LiveTileUpdatingAgent()
        {
            //InitializeErrorHandler();
        }




        #region App level Error Handler

        //static void InitializeErrorHandler()
        //{
        //    if (!isClassInitialized)
        //    {
        //        isClassInitialized = true;
        //        // Subscribe to the managed exception handler
        //        Deployment.Current.Dispatcher.BeginInvoke(delegate
        //        {
        //            Application.Current.UnhandledException += (s, e) =>
        //            {
        //                if (System.Diagnostics.Debugger.IsAttached)
        //                {
        //                    // An unhandled exception has occurred; break into the debugger
        //                    System.Diagnostics.Debugger.Break();
        //                }
        //            };
        //        });
        //    }
        //}

        #endregion




        protected async override void OnInvoke(ScheduledTask task)
        {
            Trace.Output("starting update process");
            if (!(task is PeriodicTask))
                return;

            appName = ((PeriodicTask)task).Name.Split(new[] { "pts:" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            bool isUpdateSuccessful = false;
            try
            {
                await LoadViewModelsAsync().WaitNoLongerThan(22000);
                isUpdateSuccessful = true;
                Trace.Output("COMPLETE");
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }

            DebugEx.WriteLine("Was LiveTile updated successfully?  {0}", isUpdateSuccessful);

            base.NotifyComplete();
        }

        async Task LoadViewModelsAsync()
        {
            var randomTile = SelectTileAtRandom();
            var permanentState = await new DataStorageClient().GetPermanentState();
            var userId = permanentState.UserId;
            var userClient = new Weave.User.Service.Client.Client();
            var negotiator = LiveTileNegotiatorFactory.CreateFromShellTile(
                userId, userClient, appName, randomTile);

            var motherfucker = new TaskCompletionSource<object>();
            Deployment.Current.Dispatcher.BeginInvoke(() => DoUpdate(negotiator, motherfucker));
            await motherfucker.Task;
        }

        ShellTile SelectTileAtRandom()
        {
            var tiles = ShellTile.ActiveTiles.ToList();
            var random = new System.Random();
            var picked = tiles[random.Next(0, tiles.Count)];
            return picked;
        }

        async void DoUpdate(TileNegotiatorBase ltn, TaskCompletionSource<object> motherfucker)
        {
            try
            {
                await ltn.UpdateTile();
                Trace.Output("tile updated");
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            try
            {
                ltn.UpdateLockScreen();
                Trace.Output("lock screen updated");
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            motherfucker.SetResult(new object());
        }
    }
}
