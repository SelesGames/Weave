using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;

namespace Weave.Converters
{
    public class ViewModelToDTOConverters : 
        IConverter<Outgoing.Feed, ViewModels.Feed>
    {
        public static readonly ViewModelToDTOConverters Current = new ViewModelToDTOConverters();

        public ViewModels.Feed Convert(Outgoing.Feed o)
        {
            return new ViewModels.Feed
            {
                Id = o.Id,
                Name = o.Name,
                Uri = o.Uri,
                Category = o.Category,
                ArticleViewingType = (ViewModels.ArticleViewingType)o.ArticleViewingType,
            };
        }
    }
}
