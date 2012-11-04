using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace ZuneCrawler.Core.Zest
{
    internal class ZuneCategoryCrawlerService
    {
        const string categoriesQuery = "http://catalog.zune.net/v3.2/en-US/appCategories/?clientType=WinMobile%207.1&store=Zest";
        XNamespace ns = "http://www.w3.org/2005/Atom";
        XNamespace zestns = "http://schemas.zune.net/catalog/apps/2008/02";

        public List<ZestCategory> GetCategories()
        {
            bool done = false;
            List<ZestCategory> results = null;

            while (!done)
            {
                try
                {
                    var request = WebRequest.Create(categoriesQuery) as HttpWebRequest;
                    request.KeepAlive = false;

                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (request.HaveResponse == true && response != null)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                var xml = XElement.Parse(reader.ReadToEnd());
                                results = xml
                                    .Elements(ns + "entry")
                                    .Select(category =>
                                        new ZestCategory
                                        {
                                            Id = category.Element(ns + "id").Value,
                                            Title = category.Element(ns + "title").Value,
                                            IsRoot = category.Element(zestns + "isRoot").Value
                                        })
                                    .ToList();

                                done = true;
                                reader.Close();
                            }
                        }

                        if (response != null)
                            response.Close();
                    }
                }
                catch(Exception e)
                {
                    Output.WriteLine(e);
                    done = false;
                }
            }

            return results;
        }
    }
}
