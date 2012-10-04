using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace ZuneCrawler.Core.Zest
{
    internal class ZuneAppCrawlerService
    {
        readonly string baseBrowseAppsUrl =

"http://catalog.zune.net/v3.2/{0}/apps?chunkSize={1}&clientType=WinMobile%207.1&store=Zest";

        readonly string baseBrowseAppsWithAfterMarkerUrl =

"http://catalog.zune.net/v3.2/{0}/apps?afterMarker={2}&chunkSize={1}&clientType=WinMobile%207.1&store=Zest";


        string AfterMarker;
        public bool HasMoreApps = true;
        XElement ReturnedAppsXml;

        public string LangCode { get; set; }
        public int ChunkSize { get; set; }


        public ZuneAppCrawlerService()
        {
            LangCode = "en-us";
            ChunkSize = 100;
        }

        public void GetAppsResponse()
        {
            string FullUrl;
            bool done = false;

            if (!String.IsNullOrEmpty(AfterMarker))
            {
                FullUrl = string.Format(baseBrowseAppsWithAfterMarkerUrl, LangCode, ChunkSize, AfterMarker);
            }
            else
            {
                FullUrl = string.Format(baseBrowseAppsUrl, LangCode, ChunkSize);
            }

            while (!done)
            {
                try
                {
                    var request = WebRequest.Create(FullUrl) as HttpWebRequest;
                    request.KeepAlive = false;

                    var response = request.GetResponse() as HttpWebResponse;

                    if (request.HaveResponse == true && response != null)
                    {
                        var reader = new StreamReader(response.GetResponseStream());
                        ReturnedAppsXml = XElement.Parse(reader.ReadToEnd());
                        done = true;
                    }
                }
                catch(Exception e)
                {
                    DebugEx.WriteLine(e.ToString());
                    Output.WriteLine("yeah, your connection was likely aborted");
                    done = false;
                }
            }
        }

        public IEnumerable<ZestAppData> GetAppEntries()
        {
            //first we have to parse the feed which came back
            IEnumerable<ZestAppData> entries = ReturnedAppsXml.GetZestApps();

            //now I need to get the AfterMarkerUrl from the XML feed
            var afterMarker = ReturnedAppsXml.GetAfterMarker();

            if (string.IsNullOrEmpty(afterMarker) || afterMarker == ZestService.NO_MARKER)
            {
                HasMoreApps = false;
            }
            else
            {
                AfterMarker = afterMarker;// BaseAppsUrl + afterMarker;
            }

            return entries;
        }
    }
}
