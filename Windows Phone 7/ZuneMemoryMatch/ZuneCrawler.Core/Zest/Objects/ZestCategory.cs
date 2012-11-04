
namespace ZuneCrawler.Core.Zest
{
    internal class ZestCategory
    {
        public string Id { get; set; }
        public string IsRoot { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
