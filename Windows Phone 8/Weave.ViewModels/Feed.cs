using System;

namespace Weave.ViewModels
{
    public class Feed
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Category { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }
    }
}
