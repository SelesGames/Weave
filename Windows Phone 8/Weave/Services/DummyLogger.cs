using System;

namespace weave.Services
{
    internal class DummyLogger : ILogger
    {
        public void Log(Exception exception) { }
    }
}
