using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using weave;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class CycleTileHelper
    {
        public static async Task<Uri[]> CreateImageUrisFromNews(this IEnumerable<NewsItem> news, string imagePrefix, TimeSpan downloadTimeLimit)
        {
            var imageUrls = new List<Uri>();
            var startTime = DateTime.Now;

            foreach (var newsItem in news.Where(o => o.HasImage))
            {
                var attempt = await SaveImageStreamAndReturnUri(newsItem.ImageUrl, imagePrefix, imageUrls.Count + 1);
                
                // after we download the image and saved it to isoStorage, check to see if we've gone over the total downloadTimeLimit
                var elapsed = DateTime.Now - startTime;
                if (elapsed > downloadTimeLimit)
                    break;

                if (attempt.Item1)
                {
                    imageUrls.Add(attempt.Item2);

                    // if we have 9 images, we can stop
                    if (imageUrls.Count >= 9)
                        break;
                }
            }

            if (imageUrls.Count() < 2)
                throw new Exception("not enough images were downloaded for a cycle tile");

            return imageUrls.ToArray();
        }

        static async Task<Tuple<bool, Uri>> SaveImageStreamAndReturnUri(string imageUrl, string imagePrefix, int index)
        {
            try
            {
                using (var stream = await ImageHelper.GetImageStreamAsync(imageUrl))
                {
                    if (stream.Length > 4096)
                    {
                        var url = await stream.SaveToIsoStorage(imagePrefix + index);
                        return Tuple.Create(true, url);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            return Tuple.Create(false, default(Uri));
        }
    }
}
