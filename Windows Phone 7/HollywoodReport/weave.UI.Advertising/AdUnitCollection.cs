using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace weave.UI.Advertising
{
    public class AdUnitCollection : List<string>
    {
        static Random r = new Random();
        Lazy<IObservable<Unit>> xxx;

        public string GetRandomAdUnit()
        {
            if (this.Count > 0)
                return this[r.Next(0, this.Count)];
            else
                return null;
        }

        public AdUnitCollection()
        {
            xxx = Lazy.Create(AdUnitsSetFuncCreator);
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
