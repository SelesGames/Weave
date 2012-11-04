using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;

public static class StoryboardExtensions
{
    public static IObservable<IEvent<EventArgs>> BeginWithNotification(this Storyboard storyboard)
    {
        var notification = Observable.FromEvent<EventArgs>(storyboard, "Completed").Take(1);
        storyboard.Begin();
        return notification;
    }
}
