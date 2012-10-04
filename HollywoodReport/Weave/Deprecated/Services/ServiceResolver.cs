using DependencyInjectionLite;

namespace weave
{
    public static class ServiceResolver
    {
        static IKernel kernel = new Kernel();

        public static T Get<T>()
        {
            return kernel.Get<T>();
        }

        public static T Get<T>(string key)
        {
            return kernel.Get<T>(key);
        }

        public static IKernel Current { get { return kernel; } }

        public static void Complete<T>(T item)
        {
            kernel.Complete(item);
        }
    }
}
