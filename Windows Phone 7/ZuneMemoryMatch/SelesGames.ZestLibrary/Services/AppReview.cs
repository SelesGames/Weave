using System;

namespace SelesGames.ZestLibrary
{
    public class AppReview
    {
        public DateTime Updated { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public decimal UserRating { get; set; }
    }
}
