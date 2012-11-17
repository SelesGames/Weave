
namespace System.Reactive.Concurrency
{
    public class ViewModelBackgroundScheduler
    {
        //static IScheduler instance = new EventLoopScheduler(ts => new System.Threading.Thread(ts)
        //{
        //    IsBackground = true,
        //    Name = "**## VIEWMODELBG Scheduler ##**"
        //});

        static IScheduler instance = new SingleThreadScheduler("**## VIEWMODELBG Scheduler ##**");

        public static IScheduler Instance { get { return instance; } }
    }
}
