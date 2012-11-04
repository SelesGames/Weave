using System.Concurrency;
using System.Net;

namespace weave.Services.RSS
{
    public class FeedWebRequestServiceBus
    {
        static OneThreadScheduler scheduler = new OneThreadScheduler(1);
        static HttpWebRequestQueue instance = new HttpWebRequestQueue(5, scheduler);
        public static HttpWebRequestQueue Instance { get { return instance; } }
    }
}
