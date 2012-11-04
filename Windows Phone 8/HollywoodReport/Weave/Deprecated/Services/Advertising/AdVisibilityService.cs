using System;
using System.Collections.Generic;
using System.Linq;

namespace weave
{
    public static class AdVisibilityService
    {
        static FastSubject<Unit> adsNoLongerShown = new FastSubject<Unit>();
        public static IObservable<Unit> AdsNoLongerShown
        {
            get { return adsNoLongerShown.AsObservable(); }
        }

        static int numberOfTimesAdsShown = 0;
        const int maxAdsPerSession = 1;


        public static bool AreAdsStillBeingShownAtAll
        {
            get
            {
                if (!(AppSettings.IsAddSupportedApp && AreThereAdUnitsDefined()))
                    return false;

                if (numberOfTimesAdsShown >= maxAdsPerSession)
                    return false;

                return true;
            }
        }

        static bool AreThereAdUnitsDefined()
        {
            return
                AppSettings.AdApplicationId != null &&
                AppSettings.AdUnits != null &&
                AppSettings.AdUnits.Count > 0;
        }

        public static void AdEngaged()
        {
            numberOfTimesAdsShown++;
            if (numberOfTimesAdsShown >= maxAdsPerSession)
                adsNoLongerShown.OnNext(new Unit());
        }




        //static IObservable<Location> locationBackingSource;
        //static object syncObject = new object();

        //public static IObservable<Location> GetLocationAsync()
        //{
        //    if (locationBackingSource == null)
        //    {
        //        lock (syncObject)
        //        {
        //            if (locationBackingSource == null)
        //            {
        //                locationBackingSource = ConstructGetLocationAsync();
        //            }
        //        }
        //    }
        //    return locationBackingSource;
        //}

        //static IObservable<Location> ConstructGetLocationAsync()
        //{
        //    var observer = new AsyncSubject<Location>();

        //    try
        //    {
        //        var gcw = new GeoCoordinateWatcher();
        //        IDisposable disp = Disposable.Empty;

        //        bool listen = true;
        //        gcw.PositionChanged += (s, e) =>
        //        {
        //            try
        //            {
        //                if (!listen)
        //                    return;
        //                listen = false;
        //                var location = e.Position.Location;
        //                var adLocation = new Location(location.Latitude, location.Longitude);
        //                gcw.Dispose();
        //                observer.OnNext(adLocation);
        //                observer.OnCompleted();
        //            }
        //            catch (Exception exception)
        //            {
        //                observer.OnError(exception);
        //            }
        //        };
        //        gcw.Start();
        //    }
        //    catch (Exception exception)
        //    {
        //        observer.OnError(exception);
        //    }

        //    return observer.AsObservable();
        //}
    }
}
