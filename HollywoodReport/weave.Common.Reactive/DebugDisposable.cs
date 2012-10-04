using System.Diagnostics;

namespace System.Reactive.Disposables
{
    public class DebugDisposable : IDisposable
    {
        IDisposable disposable;
        string name;

        public DebugDisposable(string name, IDisposable disposable)
        {
            this.disposable = disposable;
            this.name = name;
        }

        public void Dispose()
        {
            if (this.disposable != null)
                this.disposable.Dispose();
            Debug.WriteLine("DebugDisposable ({0}) called!", name);
        }
    }
}
