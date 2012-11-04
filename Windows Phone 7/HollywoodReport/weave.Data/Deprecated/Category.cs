using System.Collections.Generic;
using ProtoBuf;

namespace weave
{
    [ProtoContract] 
    public class Category
    {
        [ProtoMember(1)] public string Name { get; set; }
        [ProtoMember(2)] public bool IsEnabled { get; set; }
        [ProtoMember(3)] public bool UserAdded { get; set; }

        //public List<FeedSource> Feeds { get; set; }

        //public Category()
        //{
        //    Feeds = new List<FeedSource>();
        //}

        public override string ToString()
        {
            return Name;
        }
    }
}
