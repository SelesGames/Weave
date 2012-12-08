using Microsoft.Phone.Shell;
using System;
using System.Linq;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class LiveTileNegotiatorFactory
    {
        public static TileNegotiatorBase CreateFromShellTile(string appName, ShellTile tile)
        {
            var uri = tile.NavigationUri.OriginalString;

            if (string.IsNullOrEmpty(uri) || !uri.Contains("?"))
                return new StandardTileCategoryNegotiator(null, appName, tile);

            var query = uri.Split('?')[1];

            if (query.Contains("feedId"))
            {
                var feedId = query.Split(new string[] { "feedId=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                return new StandardTileFeedNegotiator(Guid.Parse(feedId), appName, tile);
            }

            else if (query.Contains("mode"))
            {
                var category = query
                    .Split('&')
                    .Single(o => o.Contains("header"))
                    .Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .SingleOrDefault();

                return new StandardTileCategoryNegotiator(category, appName, tile);
            }

            else
                return null;
        }
    }

}
