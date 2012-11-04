using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace weave
{
    public class AdUnitsProviderService
    {
        const string AD_UNITS_URL = "http://weavestorage.blob.core.windows.net/settings/adunits";

        LazySingleton<IObservable<List<string>>> adUnitsProvider;

        public AdUnitsProviderService()
        {
            adUnitsProvider = LazySingleton.Create(CreateGetAdUnitsAsyncFunc);
        }

        IObservable<List<string>> CreateGetAdUnitsAsyncFunc()
        {
            var backingSource = new AsyncSubject<List<string>>();

            try
            {
                AD_UNITS_URL.ToUri().ToWebRequest().GetWebResponseAsync().Subscribe(
                    o =>
                    {
                        try
                        {
                            var s = o.GetResponseStreamAsString();
                            using (var sr = new StringReader(s))
                            {
                                var lines = sr.ReadLines().ToList();
                                sr.Close();
                                backingSource.OnNext(lines);
                                backingSource.OnCompleted();
                            }
                        }
                        catch (Exception exception)
                        {
                            backingSource.OnError(exception);
                        }
                    },
                    backingSource.OnError);
            }
            catch (Exception exception)
            {
                backingSource.OnError(exception);
            }

            return backingSource.AsObservable();
        }

        public IObservable<List<string>> GetAdUnitsAsync()
        {
            return adUnitsProvider.Get();
        }
    }
}

namespace System.IO
{
    public static class StringReaderExtensions
    {
        public static IEnumerable<string> ReadLines(this StringReader sr)
        {
            bool linesRemain = true;
            while (linesRemain)
            {
                var line = sr.ReadLine();
                if (line != null)
                {
                    yield return line;
                }
                else
                    linesRemain = false;
            }
        }
    }
}
