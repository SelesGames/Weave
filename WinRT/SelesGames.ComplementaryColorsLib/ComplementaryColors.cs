
namespace Windows.UI
{
    ///
    /// To compute complementary color as in Kuler tool
    /// http://kuler.adobe.com/#create/fromacolor
    /// 
    public struct ComplementaryColors
    {
        public Color OriginalColorDarker { get; set; }
        public Color OriginalColorLighter { get; set; }
        public Color ComplementaryColorDarker { get; set; }
        public Color ComplementaryColorLighter { get; set; }
    }
}
