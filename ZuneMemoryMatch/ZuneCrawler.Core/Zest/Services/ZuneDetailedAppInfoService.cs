using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace ZuneCrawler.Core.Zest
{
    public class ZuneDetailedAppInfoService
    {
        const string FEATURED_APPS = "http://catalog.zune.net/v3.2/en-US/clientTypes/WinMobile%207.0/hubTypes/apps/hub/?store=zest";
        static XNamespace a = "http://www.w3.org/2005/Atom";
        const string x = "http://schemas.zune.net/catalog/apps/2008/02";
        const string detailedInfoUri = "http://catalog.zune.net/v3.2/en-US/apps/{0}/?version=latest&clientType=WinMobile%207.0&store=Zest";

        static string ExtractImageUrl(string p)
        {
            return p != null ? p.Replace("urn:uuid:", string.Empty) : null;
        }


        public static IObservable<ExtendedZuneAppInfo> GetExtendedZuneAppInfo(string appId)
        {
            return string.Format(detailedInfoUri, appId).ToUri().ToWebRequest().GetWebResponseFullyAsync()
                .Select(response =>
                {
                    try
                    {
                        var result = response.GetResponseStreamAsString();
                        var element = XElement.Parse(result);
                        var extendedInfo =
                            new ExtendedZuneAppInfo
                            {
                                BackgroundImage = Try(() => ExtractImageUrl(element.Element(XName.Get("backgroundImage", x)).Element(XName.Get("id", x)).ValueOrDefault()), string.Empty),
                                ScreenShotIds = element
                                    .Element(XName.Get("screenshots", x))
                                    .Elements(XName.Get("screenshot", x))
                                    .Select(o => ExtractImageId(o.Element(XName.Get("id", x)).ValueOrDefault()))
                                    .ToList()
                            };

                        return extendedInfo;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        response.Close();
                        ((IDisposable)response).Dispose();
                    }
                });
        }

        static string ExtractImageId(string p)
        {
            if (p == null)
                return null;

            return p.Replace("urn:uuid:", string.Empty);
        }

        static T Try<T>(Func<T> valFactory, T defaultVal = default(T))
        {
            try
            {
                return valFactory();
            }
            catch (Exception)
            {
                return defaultVal;
            }
        }
    }

    public class ExtendedZuneAppInfo
    {
        public string BackgroundImage { get; set; }
        public List<string> ScreenShotIds { get; set; }
    }
}
