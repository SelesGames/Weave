
namespace SelesGames
{
    public interface IServiceResolver
    {
        T Get<T>();
        T Get<T>(string key);
    }

    public static class ServiceResolver
    {
        static IServiceResolver internalResolver;

        public static void SetInternalResolver(IServiceResolver internalResolver)
        {
            ServiceResolver.internalResolver = internalResolver;
        }

        public static T Get<T>()
        {
            if (internalResolver == null)
                return default(T);

            return internalResolver.Get<T>();
        }

        public static T Get<T>(string key)
        {
            if (internalResolver == null)
                return default(T);

            return internalResolver.Get<T>(key);
        }
    }
}
