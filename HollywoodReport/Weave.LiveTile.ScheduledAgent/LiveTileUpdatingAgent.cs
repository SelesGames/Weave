using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace Weave.LiveTile.ScheduledAgent
{
    public class LiveTileUpdatingAgent : ScheduledTaskAgent
    {
        //static volatile bool isClassInitialized;

        string tileUrl;


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

            tileUrl = ((PeriodicTask)task).Name;

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
            var negotiator = LiveTileNegotiatorFactory.CreateFromShellTile(randomTile);

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

        async void DoUpdate(LiveTileNegotiatorBase ltn, TaskCompletionSource<object> motherfucker)
        {
            await ltn.UpdateTile();            
            Trace.Output("tile updated");
            motherfucker.SetResult(new object());
        }
    }
}
