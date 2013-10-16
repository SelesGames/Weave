using SelesGames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels.Contracts.Client;

namespace weave
{
    public class PanoramaViewModel
    {
        IUserCache userCache = ServiceResolver.Get<IUserCache>();

        public ObservableCollection<NewsItemGroup> MostViewed { get; private set; }

        public PanoramaViewModel()
        {
            MostViewed = new ObservableCollection<NewsItemGroup>();
        }

        static Random r = new Random();

        public void LoadMostViewedAsync()
        {
            var user = userCache.Get();
            var feeds = user.Feeds;

            var temp = new List<NewsItemGroup>();

            var sources = feeds.GetAllSources().ToList();
            
            var history = AppSettings.Instance.PermanentState.Get().WaitOnResult().RunHistory.GetTallies();
            var historyEnumerator = history.GetEnumerator();
            while (temp.Count < 4 && historyEnumerator.MoveNext())
            {
                var mostViewed = historyEnumerator.Current;
                var matching = sources.FirstOrDefault(o => o.DisplayName == mostViewed.Label);
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
                o.ImageSource = o.GetTeaserPicImageUrl();
                MostViewed.Add(o);
            }

            sw.Stop();
            DebugEx.WriteLine("most viewed took {0} ms to figure out", sw.ElapsedMilliseconds);
        }
    }
}
