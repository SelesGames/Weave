using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Controls;

namespace weave
{
    public static class PanoramaInjectionService
    {
        static List<Func<PanoramaItem>> injections = new List<Func<PanoramaItem>>();

        public static void Register(Func<PanoramaItem> piCreator)
        {
            injections.Add(piCreator);
        }

        public static IEnumerable<PanoramaItem> GetAll()
        {
            return injections.Select(f => f());
        }

        public static int Count { get { return injections.Count; } }
    }
}
