using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls
{
    public static class ControlsExtensions
    {
        public static IObservable<EventPattern<RoutedEventArgs>> GetClick(this ButtonBase button)
        {
            return Observable.FromEventPattern<RoutedEventArgs>(button, "Click");
        }
    }
}
