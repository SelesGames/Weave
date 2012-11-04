using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace SandboxTestBedApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }
    }

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