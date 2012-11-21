using System;
using Windows.UI.Core;
using Windows.UI.Xaml;

public static class GlobalDispatcher
{
    public static DispatcherAdapter Current { get { return new DispatcherAdapter(Window.Current.Dispatcher); } }
}

public class DispatcherAdapter
{
    CoreDispatcher dispatcher;

    public DispatcherAdapter(CoreDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;
    }

    public void BeginInvoke(Action a)
    {
        dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => a());
    }
}
