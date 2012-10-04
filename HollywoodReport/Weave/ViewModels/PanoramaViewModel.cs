using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SelesGames;
using weave.Data;

namespace weave
{
    public class PanoramaViewModel
    {
        Weave4DataAccessLayer dal;
        IEnumerable<CategoryOrLooseFeedViewModel> previousSources = new List<CategoryOrLooseFeedViewModel>();

        public ObservableCollection<CategoryOrLooseFeedViewModel> Sources { get; set; }
        public ObservableCollection<CategoryOrLooseFeedViewModel> MostViewed { get; set; }

        public PanoramaViewModel()
        {
            dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();

            Sources = new ObservableCollection<CategoryOrLooseFeedViewModel>();
            MostViewed = new ObservableCollection<CategoryOrLooseFeedViewModel>();
        }

        public async Task LoadSourcesAsync()
        {
            var feeds = await dal.Feeds.Get();
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

        public async Task LoadMostViewedAsync()
        {
            var feeds = await dal.Feeds.Get();

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
                NewsItem firstNewsItem = null;

                if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Category)
                {
                    if (o.Name != null && o.Name.Equals("all news", StringComparison.OrdinalIgnoreCase))
                        firstNewsItem = feeds.AllOrderedNews().FirstOrDefault(x => x.HasImage);
                    else
                        firstNewsItem = feeds.OfCategory(o.Name).AllOrderedNews().FirstOrDefault(x => x.HasImage);
                }
                else if (o.Type == CategoryOrLooseFeedViewModel.CategoryOrFeedType.Feed)
                {
                    firstNewsItem = feeds.Where(x => x.FeedName.Equals(o.Name, StringComparison.OrdinalIgnoreCase)).Take(1).AllOrderedNews().FirstOrDefault(x => x.HasImage);
                }
                if (firstNewsItem != null && firstNewsItem.HasImage)
                    o.Source = firstNewsItem.ImageUrl;

                MostViewed.Add(o);
            }

            sw.Stop();
            DebugEx.WriteLine("most viewed took {0} ms to figure out", sw.ElapsedMilliseconds);
        }
    }
}
