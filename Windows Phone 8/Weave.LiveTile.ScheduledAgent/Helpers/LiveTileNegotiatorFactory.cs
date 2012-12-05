using Microsoft.Phone.Shell;
using System;
using System.Linq;

namespace Weave.LiveTile.ScheduledAgent
{
    public static class LiveTileNegotiatorFactory
    {
        public static LiveTileNegotiatorBase CreateFromShellTile(string appName, ShellTile tile)
        {
            var uri = tile.NavigationUri.OriginalString;

            if (string.IsNullOrEmpty(uri) || !uri.Contains("?"))
                return new CategoryLiveTileNegotiator(null, appName, tile);

            var query = uri.Split('?')[1];

            if (query.Contains("feedId"))
            {
                var feedId = query.Split(new string[] { "feedId=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                return new FeedLiveTileNegotiator(Guid.Parse(feedId), appName, tile);
            }

            else if (query.Contains("mode"))
            {
                var category = query
                    .Split('&')
                    .Single(o => o.Contains("header"))
                    .Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .SingleOrDefault();

                return new CategoryLiveTileNegotiator(category, appName, tile);
            }

            else
                return null;
        }

        //static IEnumerable<KeyValuePair<string, string>> GetParameters(string parameterString)
        //{
        //    return parameterString
        //        .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
        //        .Select(o => o.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
        //        .Select(o => new KeyValuePair<string, string>(o.FirstOrDefault(), o.Skip(1).SingleOrDefault()));
        //}
    }

}
