using ProtoBuf;
using System;
using System.Collections.Generic;

namespace weave
{
    [ProtoContract] 
    public class FeedSource
    {
        [ProtoMember(1)]    public Guid Id { get; set; }
        [ProtoMember(2)]    public string FeedName { get; set; }
        [ProtoMember(3)]    public string FeedUri { get; set; }
        [ProtoMember(4)]    public string Category { get; set; }
        [ProtoMember(5)]    public string Etag { get; set; }
        [ProtoMember(6)]    public string LastModified { get; set; }
        [ProtoMember(7)]    public string MostRecentNewsItemPubDate { get; set; }
        [ProtoMember(8)]    public DateTime LastRefreshedOn { get; set; }
        [ProtoMember(9)]    public Guid NewsHash { get; set; }
        [ProtoMember(10)]   public ArticleViewingType ArticleViewingType { get; set; }
        [ProtoMember(11)]   public List<UpdateParameters> UpdateHistory { get; set; }

        public List<NewsItem> News { get; set; }
 



        #region Overrides of the Equals and GetHashCode functions for determining equality

        public override bool Equals(object obj)
        {
            var that = obj as FeedSource;
            if (that == null || this.FeedUri == null)
                return false;

            return this.FeedUri.Equals(that.FeedUri, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return FeedUri != null ? FeedUri.GetHashCode() : -1;
        }

        #endregion
    }
}