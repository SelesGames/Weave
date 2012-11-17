
namespace System.Reactive
{
    public static class EventPattern
    {
        public static EventPattern<T> Create<T>(object sender, T eventArgs) where T : EventArgs
        {
            return new EventPattern<T>(sender, eventArgs);
        }
    }
}
