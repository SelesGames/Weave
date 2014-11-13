using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Weave.Services.User.DTOs.ServerOutgoing;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class CycleTileHelper
    {
        public static async Task<Uri[]> CreateImageUrisFromNews(this IEnumerable<string> urls, string imagePrefix, TimeSpan downloadTimeLimit)
        {
            var imageUrls = new List<Uri>();
            var startTime = DateTime.Now;

            foreach (var url in urls.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                var attempt = await SaveImageStreamAndReturnUri(url, imagePrefix, imageUrls.Count + 1);
                
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
                var response = await new HttpClient().GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                if (
                    response.Content != null &&
                    response.Content.Headers != null &&
                    response.Content.Headers.ContentLength.HasValue &&
                    response.Content.Headers.ContentLength > 4096)
                {
                    using (var stream = new MemoryStream())
                    {
                        await response.Content.CopyToAsync(stream);
                        stream.Position = 0;

                        if (stream.Length > 4096)
                        {
                            var url = await stream.SaveToIsoStorage(imagePrefix + index);
                            return Tuple.Create(true, url);
                        }
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
