﻿using System.Collections.Generic;
using ProtoBuf;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    public enum FeedResultStatus
    {
        OK,
        Failed,
        Unmodified
    }

    [ProtoContract]
    public class FeedResult
    {
        [ProtoMember(1)] public FeedResultStatus Status { get; set; }
        [ProtoMember(2)] public string Id { get; set; }
        [ProtoMember(3)] public string MostRecentNewsItemPubDate { get; set; }
        [ProtoMember(4)] public string OldestNewsItemPubDate { get; set; }
        [ProtoMember(5)] public string Etag { get; set; }  //optional
        [ProtoMember(6)] public string LastModified { get; set; }  //optional
        [ProtoMember(7)] public List<NewsItem> News { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: Id:{1}  MostRecent:{2}  Oldest:{3}  Etag:{4}  LastModified:{5}  NewsCount:{6}",
                Status.ToString(),
                Id,
                MostRecentNewsItemPubDate ?? "{null}",
                OldestNewsItemPubDate ?? "{null}",
                Etag ?? "{null}",
                LastModified ?? "{null}",
                News == null ? "{null}" : News.Count.ToString());
        }
    }
}
