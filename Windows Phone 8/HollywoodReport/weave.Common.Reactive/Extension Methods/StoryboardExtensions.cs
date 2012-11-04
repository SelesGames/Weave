using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Animation;

public static class StoryboardExtensions
{
    public static IObservable<EventPattern<EventArgs>> BeginWithNotification(this Storyboard storyboard)
    {
        var notification = Observable.FromEventPattern<EventArgs>(storyboard, "Completed").Take(1);
        storyboard.Begin();
        return notification;
    }
}
