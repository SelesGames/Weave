using System;

namespace Weave.Services
{
    internal class DummyLogger : ILogger
    {
        public void Log(Exception exception) { }
    }
}
