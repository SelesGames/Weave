using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Microsoft.Phone.Shell
{
    public static class ApplicationBarExtensions
    {
        public static IObservable<EventPattern<EventArgs>> GetClick(this ApplicationBarIconButton button)
        {
            return Observable.FromEventPattern<EventArgs>(button, "Click");
        }

        public static IObservable<EventPattern<EventArgs>> GetClick(this ApplicationBarMenuItem button)
        {
            return Observable.FromEventPattern<EventArgs>(button, "Click");
        }
    }
}
