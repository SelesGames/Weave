
namespace System.Reactive.Concurrency
{
    public static class BackgroundTimer
    {
        //public static readonly IScheduler Scheduler = new EventLoopScheduler(ts => 
        //    new System.Threading.Thread(ts)
        //    { 
        //        IsBackground = true, 
        //        Name = "**## BACKGROUND TIMER ##**",
        //    });
        public static readonly IScheduler Scheduler = new SingleThreadScheduler("**## BACKGROUND TIMER ##**");
    }
}
