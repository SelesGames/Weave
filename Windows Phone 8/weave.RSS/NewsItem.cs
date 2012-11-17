using System;

namespace Weave.RSS
{
    public class NewsItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime PublishDateTime { get; set; }
        public string ImageUrl { get; set; }

        // added 10/10/11
        public string YoutubeId { get; set; }
        public string VideoUri { get; set; }
        public string PodcastUri { get; set; }
        public string ZuneAppId { get; set; }
    }
}
