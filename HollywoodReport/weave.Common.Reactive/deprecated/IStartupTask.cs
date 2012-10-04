using System;
using System.Reactive;

namespace weave
{
    public interface IStartupTask
    {
        IObservable<Unit> StartupComplete { get; }
    }
}
