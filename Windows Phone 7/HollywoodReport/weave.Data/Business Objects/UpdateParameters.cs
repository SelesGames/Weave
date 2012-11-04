using System;
using ProtoBuf;

namespace weave
{
    [ProtoContract] 
    public class UpdateParameters
    {
        [ProtoMember(1)]    public string Etag { get; set; }
        [ProtoMember(2)]    public string LastModified { get; set; }
        [ProtoMember(3)]    public string MostRecentNewsItemPubDate { get; set; }
        [ProtoMember(4)]    public Guid NewsHash { get; set; }

        public override string ToString()
        {
            return string.Format("Etag: {0}, LastModified: {1}, MostRecentNewsItemPubdate: {2}, NewsHash: {3}",
                Etag, LastModified, MostRecentNewsItemPubDate, NewsHash);
        }
    }
}
