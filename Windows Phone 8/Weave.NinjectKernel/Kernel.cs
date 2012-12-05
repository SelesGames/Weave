using Ninject;
using System;
using weave.Data;
using Weave.FeedLibrary;
using Weave.Mobilizer.Client;
using Weave.RSS;

namespace Weave.NinjectKernel
{
    public class Kernel : StandardKernel
    {
        string assemblyName;

        public Kernel(string assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        protected override void AddComponents()
        {
            base.AddComponents();

            Bind<NewsServer>().ToSelf().InSingletonScope();
            Bind<Weave4DataAccessLayer>().ToSelf().InSingletonScope();

            Bind<BundledLibrary>().ToMethod(_ => new BundledLibrary(assemblyName)).InTransientScope();
            Bind<Formatter>().ToSelf().InSingletonScope();
            Bind<ILogger>().ToConstant(new dummyLogger()).InSingletonScope();
        }

        class dummyLogger : ILogger
        {
            public void Log(Exception exception) { }
        }
    }
}
