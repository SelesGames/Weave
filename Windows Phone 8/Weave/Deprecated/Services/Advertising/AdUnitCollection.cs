using System;
using System.Collections.Generic;
using System.Linq;

namespace weave
{
    public class AdUnitCollection : List<string>
    {
        static Random r = new Random();
        LazySingleton<IObservable<Unit>> xxx;

        public string GetRandomAdUnit()
        {
            if (this.Count > 0)
                return this[r.Next(0, this.Count)];
            else
                return null;
        }

        public AdUnitCollection()
        {
            xxx = LazySingleton.Create(AdUnitsSetFuncCreator);
        }

        public IObservable<Unit> AdUnitsSet()
        {
            return xxx.Get();
        }

        public IObservable<Unit> AdUnitsSetFuncCreator()
        {
            var observer = new AsyncSubject<Unit>();

            try
            {
                new AdUnitsProviderService().GetAdUnitsAsync().Subscribe(
                    o =>
                    {
                        SetAdUnits(o);
                        observer.OnNext(new Unit());
                        observer.OnCompleted();
                    },
                    exception =>
                    {
                        HandleException(exception);
                        observer.OnNext(new Unit());
                        observer.OnCompleted();
                    });
            }
            catch (Exception)
            {
                observer.OnNext(new Unit());
                observer.OnCompleted();
            }

            return observer.AsObservable();
        }

        void SetAdUnits(List<string> adUnits)
        {
            this.Clear();
            this.AddRange(adUnits);
        }

        void HandleException(Exception exception)
        {
        }
    }
}
