using System.Reactive.Concurrency;

namespace weave
{
    public static class Schedulers
    {
        static IScheduler imageDownloaderScheduler = new SingleThreadScheduler("Image Download Thread");
        //static IScheduler standardWebRequestScheduler = new SingleThreadScheduler("Image Download Thread");

        public static IScheduler ImageDownloaderScheduler { get { return imageDownloaderScheduler; } }
    }
}
