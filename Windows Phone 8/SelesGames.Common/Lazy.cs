
namespace System
{
    public static class Lazy
    {
        public static Lazy<T> Create<T>(Func<T> creator, bool isThreadSafe = true)
        {
            return new Lazy<T>(creator, isThreadSafe);
        }

        public static T Get<T>(this Lazy<T> lazy)
        {
            return lazy.Value;
        }
    }
}
