using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using weave;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class CycleTileHelper
    {
        public static async Task<Uri[]> CreateImageUrisFromNews(this IEnumerable<NewsItem> news, TimeSpan downloadTimeLimit)
        {
            var imageUrls = new List<Uri>();
            var startTime = DateTime.Now;

            foreach (var newsItem in news.Where(o => o.HasImage))
            {
                var attempt = await SaveImageStreamAndReturnUri(newsItem.ImageUrl, imageUrls.Count + 1);
                if (attempt.Item1)
                {
                    imageUrls.Add(attempt.Item2);

                    var elapsed = DateTime.Now - startTime;
                    if (imageUrls.Count >= 9 || elapsed > downloadTimeLimit)
                        break;
                }
            }

            if (imageUrls.Count() < 2)
                throw new Exception("not enough images were downloaded for a cycle tile");

            return imageUrls.ToArray();
        }

        static async Task<Tuple<bool, Uri>> SaveImageStreamAndReturnUri(string imageUrl, int index)
        {
            try
            {
                using (var stream = await ImageHelper.GetImageStreamAsync(imageUrl))
                {
                    DebugEx.WriteLine("IMAGE LENGTH: {0}", stream.Length);
                    if (stream.Length > 4056)
                    {
                        var url = await stream.SaveToIsoStorage("photo" + index);
                        return Tuple.Create(true, url);
                    }
                }
            }
            catch { }
            return Tuple.Create(false, default(Uri));
        }
    }
}
