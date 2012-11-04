using Microsoft.Phone.Info;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class Trace
    {
        public static void Output(string message)
        {
            //ToastHelper.ShowToast("Weave", string.Format("{0} {1}", message, OutputMemory()), null);
        }

        static long mb = 1024 * 1024;

        static string OutputMemory()
        {
            var limit = DeviceStatus.ApplicationMemoryUsageLimit / mb;
            var current = DeviceStatus.ApplicationCurrentMemoryUsage / mb;
            return string.Format("{0}mb / {1}mb", current, limit);
        }
    }
}
