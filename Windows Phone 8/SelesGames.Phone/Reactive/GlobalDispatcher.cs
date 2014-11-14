using System.Windows;
using System.Windows.Threading;

public static class GlobalDispatcher
{
    public static Dispatcher Current { get { return Deployment.Current.Dispatcher; } }
}
