using System;

namespace SelesGames.WP.IsoStorage
{
    internal static class Lazy
    {
        public static Lazy<T> Create<T>(Func<T> creator) where T : class
        {
            return new Lazy<T>(creator);
        }
    }

    internal class Lazy<T> where T : class
    {
        object syncObject = new object();
        T instance = null;
        Func<T> creator;

        public Lazy(Func<T> creator)
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
