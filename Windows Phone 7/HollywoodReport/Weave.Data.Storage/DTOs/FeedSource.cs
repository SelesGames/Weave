using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;
using weave;

namespace Weave.Data.Storage.DTOs
{
    [DataContract]
    [ProtoContract] 
    public class FeedSource
    {
        [DataMember][ProtoMember(1)]    public Guid Id { get; set; }
        [DataMember][ProtoMember(2)]    public string FeedName { get; set; }
        [DataMember][ProtoMember(3)]    public string FeedUri { get; set; }
        [DataMember][ProtoMember(4)]    public string Category { get; set; }
        [DataMember][ProtoMember(5)]    public string Etag { get; set; }
        [DataMember][ProtoMember(6)]    public string LastModified { get; set; }
        [DataMember][ProtoMember(7)]    public string MostRecentNewsItemPubDate { get; set; }
        [DataMember][ProtoMember(8)]    public DateTime LastRefreshedOn { get; set; }
        [DataMember][ProtoMember(9)]    public Guid NewsHash { get; set; }
        [DataMember][ProtoMember(10)]   public ArticleViewingType ArticleViewingType { get; set; }
        [DataMember][ProtoMember(11)]   public List<UpdateParameters> UpdateHistory { get; set; }
    }
}