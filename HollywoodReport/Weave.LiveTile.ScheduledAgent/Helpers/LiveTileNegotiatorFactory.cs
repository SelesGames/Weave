using System;
using Microsoft.Phone.Shell;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class LiveTileNegotiatorFactory
    {
        public static LiveTileNegotiatorBase CreateFromShellTile(ShellTile tile)
        {
            var uri = tile.NavigationUri.OriginalString;

            if (string.IsNullOrEmpty(uri) || !uri.Contains("?"))
                return new CategoryLiveTileNegotiator("all news", tile);

            var query = uri.Split('?')[1];

            if (query.Contains("feedId"))
            {
                var feedId = query.Split(new string[] { "feedId=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                return new FeedLiveTileNegotiator(Guid.Parse(feedId), tile);
            }

            else if (query.Contains("mode"))
            {
                return new CategoryLiveTileNegotiator("all news", tile);
            }

            else
                return null;
        }
    }
}
