using System.Net;
using System.Reactive.Concurrency;

namespace weave.Services.RSS
{
    public class FeedWebRequestServiceBus
    {
        static SingleThreadScheduler scheduler = new SingleThreadScheduler("**## F W R S B ##**");

        //static IScheduler scheduler = new EventLoopScheduler(ts => 
        //    new System.Threading.Thread(ts) { IsBackground = true, Name = "**## F W R S B ##**" });

        static HttpWebRequestQueue instance = new HttpWebRequestQueue(5, scheduler, scheduler);

        public static HttpWebRequestQueue Instance { get { return instance; } }
    }
}
