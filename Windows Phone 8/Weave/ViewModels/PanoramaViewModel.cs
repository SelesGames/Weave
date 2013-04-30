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
        IEnumerable<CategoryOrLooseFeedViewModel> previousSources = new List<CategoryOrLooseFeedViewModel>();

        public ObservableCollection<CategoryOrLooseFeedViewModel> Sources { get; set; }
        public ObservableCollection<CategoryOrLooseFeedViewModel> MostViewed { get; set; }

        public PanoramaViewModel()
        {
            Sources = new ObservableCollection<CategoryOrLooseFeedViewModel>();
            MostViewed = new ObservableCollection<CategoryOrLooseFeedViewModel>();
        }

        public void LoadSourcesAsync()
        {
            var feeds = userCache.Get().Feeds;
            var sources = feeds.GetAllSources().ToList();

            bool areItemsNew = !Enumerable.SequenceEqual(sources, previousSources);

            if (areItemsNew)
            {
                Sources.Clear();
                foreach (var source in sources)
                    Sources.Add(source);
            }
        }

        //public async Task RefreshUpdateNewsCountForAllSourcesAsync()
        //{
        //    if (Sources == null)
        //        return;

        //    await Sources.Select(o => o.UpdateNewsCountAfterRefreshAsync());
        //}

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
                CategoryOrFeedTeaserImage firstNewsItem = null;

                if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category)
                {
                    if (o.Name != null && o.Name.Equals("all news", StringComparison.OrdinalIgnoreCase))
                        firstNewsItem = user.TeaserImages.FirstOrDefault(x => x.Category == null);
                    else
                        firstNewsItem = user.TeaserImages.FirstOrDefault(x => x.Category.Equals(o.Name, StringComparison.OrdinalIgnoreCase));
                }
                else if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed)
                {
                    firstNewsItem = user.TeaserImages.FirstOrDefault(x => x.FeedId == o.FeedId);
                }
                if (firstNewsItem != null && !string.IsNullOrEmpty(firstNewsItem.ImageUrl))
                    o.Source = firstNewsItem.ImageUrl;

                MostViewed.Add(o);
            }

            sw.Stop();
            DebugEx.WriteLine("most viewed took {0} ms to figure out", sw.ElapsedMilliseconds);
        }
    }
}
