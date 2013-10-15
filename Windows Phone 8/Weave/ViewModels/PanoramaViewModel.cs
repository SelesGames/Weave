using SelesGames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;

namespace weave
{
    public class PanoramaViewModel
    {
        IUserCache userCache = ServiceResolver.Get<IUserCache>();

        public ObservableCollection<CategoryOrLooseFeedViewModel> MostViewed { get; private set; }
        public ObservableCollection<Feed> Feeds { get; private set; }

        public PanoramaViewModel()
        {
            MostViewed = new ObservableCollection<CategoryOrLooseFeedViewModel>();
            Feeds = new ObservableCollection<Feed>();
        }

        static Random r = new Random();

        public void LoadMostViewedAsync()
        {
            var user = userCache.Get();
            var feeds = user.Feeds;

            var temp = new List<CategoryOrLooseFeedViewModel>();

            var sources = feeds.GetAllSources().ToList();
            
            var history = AppSettings.Instance.PermanentState.Get().WaitOnResult().RunHistory.GetTallies();
            var historyEnumerator = history.GetEnumerator();
            while (temp.Count < 4 && historyEnumerator.MoveNext())
            {
                var mostViewed = historyEnumerator.Current;
                var matching = sources.FirstOrDefault(o => o.Name == mostViewed.Label);
                if (matching != null)
                {
                    temp.Add(matching);
                    sources.Remove(matching);
                }
            }

            var sourcesEnumerator = sources.GetEnumerator();
            while (temp.Count < 4 && sourcesEnumerator.MoveNext())
            {
                var source = sourcesEnumerator.Current;
                temp.Add(source);
            }

            MostViewed.Clear();

            var sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (var o in temp)
            {
                string pic = null;

                if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category)
                {
                    List<Feed> eligibleFeeds;

                    if (o.Name != null && o.Name.Equals("all news", StringComparison.OrdinalIgnoreCase))
                        eligibleFeeds = feeds.ToList();
                    else
                        eligibleFeeds = feeds.OfCategory(o.Name).ToList();

                    // TODO: RANDOMLY SELECT ONE FEED TO DISPLAY
                    pic = eligibleFeeds.First().TeaserImageUrl;
                }
                else if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed)
                {
                    pic = feeds.Where(x => x.Name.Equals(o.Name, StringComparison.OrdinalIgnoreCase)).Select(z=> z.TeaserImageUrl).FirstOrDefault();
                }
                o.ImageSource = pic;

                MostViewed.Add(o);
            }

            sw.Stop();
            DebugEx.WriteLine("most viewed took {0} ms to figure out", sw.ElapsedMilliseconds);
        }

        public void LoadFeeds()
        {
            var feeds = userCache.Get().Feeds;

            Feeds.Clear();
            foreach (var feed in feeds.Where(o => !string.IsNullOrWhiteSpace(o.TeaserImageUrl)))
                Feeds.Add(feed);
        }
    }
}
