using System;
using System.Threading;


public class DebugEx
{
    public static void WriteLine(string format, params object[] args)
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            return;

        string timestamp = string.Format("THREAD: {0}, AT: {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("hh:mm:ss.fff tt"));
        System.Diagnostics.Debug.WriteLine(timestamp + "\t" + format, args);
    }
}
