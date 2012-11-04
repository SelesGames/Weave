
namespace System
{
    public static class LazySingleton
    {
        public static LazySingleton<T> Create<T>(Func<T> creator) where T : class
        {
            return new LazySingleton<T>(creator);
        }
    }

    public class LazySingleton<T> where T : class
    {
        object syncObject = new object();
        T instance = null;
        Func<T> creator;

        public LazySingleton(Func<T> creator)
        {
            this.creator = creator;
        }

        public T Get()
        {
            if (instance == null)
            {
                var temp = creator();
                lock (syncObject)
                {
                    if (instance == null)
                        instance = temp;
                }
            }
            return instance;
        }

        public void Refresh()
        {
            lock (syncObject)
            {
                instance = creator();
            }
        }
    }
}
