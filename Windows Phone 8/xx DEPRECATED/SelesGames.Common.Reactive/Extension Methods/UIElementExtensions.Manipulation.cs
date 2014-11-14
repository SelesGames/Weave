using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace System.Windows
{
    public static partial class UIElementExtensions
    {
        public static IObservable<EventPattern<ManipulationStartedEventArgs>> GetManipulationStarted(this UIElement el)
        {
            return Observable.FromEventPattern<EventHandler<ManipulationStartedEventArgs>, ManipulationStartedEventArgs>
                (h => new EventHandler<ManipulationStartedEventArgs>(h),
                    h => el.ManipulationStarted += h,
                    h => el.ManipulationStarted -= h);
        }

        public static IObservable<EventPattern<ManipulationDeltaEventArgs>> GetManipulationDelta(this UIElement el)
        {
            return Observable.FromEventPattern<EventHandler<ManipulationDeltaEventArgs>, ManipulationDeltaEventArgs>
                (h => new EventHandler<ManipulationDeltaEventArgs>(h),
                    h => el.ManipulationDelta += h,
                    h => el.ManipulationDelta -= h);
        }

        public static IObservable<EventPattern<ManipulationCompletedEventArgs>> GetManipulationCompleted(this UIElement el)
        {
            return Observable.FromEventPattern<EventHandler<ManipulationCompletedEventArgs>, ManipulationCompletedEventArgs>
                (h => new EventHandler<ManipulationCompletedEventArgs>(h),
                    h => el.ManipulationCompleted += h,
                    h => el.ManipulationCompleted -= h);
        }
    }
}
