
namespace System
{
    public static class Lazy
    {
        public static Lazy<T> Create<T>(Func<T> creator, bool isThreadSafe = true)// where T : class
        {
            return new Lazy<T>(creator, isThreadSafe);
        }

        public static T Get<T>(this Lazy<T> lazy)
        {
            return lazy.Value;
        }
    }

    //public class Lazy<T> where T : class
    //{
    //    object syncObject = new object();
    //    T instance = null;
    //    Func<T> creator;

    //    public Lazy(Func<T> creator)
    //    {
    //        this.creator = creator;
    //    }

    //    public T Get()
    //    {
    //        if (instance == null)
    //        {
    //            var temp = creator();
    //            lock (syncObject)
    //            {
    //                if (instance == null)
    //                    instance = temp;
    //            }
    //        }
    //        return instance;
    //    }

    //    public void Refresh()
    //    {
    //        lock (syncObject)
    //        {
    //            instance = creator();
    //        }
    //    }
    //}
}
