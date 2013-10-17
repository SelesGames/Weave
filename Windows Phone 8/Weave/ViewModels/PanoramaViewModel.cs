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
        public ObservableCollection<NewsItemGroup> MostViewed { get; private set; }

        public PanoramaViewModel()
        {
            MostViewed = new ObservableCollection<NewsItemGroup>();
        }

        //static Random r = new Random();

        public void LoadMostViewedAsync()
        {
            MostViewed.Clear();

            var feedsListener = ServiceResolver.Get<FeedsListenerViewModel>();
            var sources = feedsListener.Feeds;
            
            var history = AppSettings.Instance.PermanentState.Get().WaitOnResult().RunHistory.GetTallies();

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var x = from source in sources
                    join h in history on source.DisplayName equals h.Label
                    let tempX = new { Source = source, LabelTally = h }
                    orderby tempX.LabelTally.Count descending
                    select tempX.Source;

            var mostViewed = x.Union(sources).Take(4).ToList();

            foreach (var o in mostViewed)
            {
                o.ImageSource = o.GetTeaserPicImageUrl();
                MostViewed.Add(o);
            }

            sw.Stop();
            DebugEx.WriteLine("most viewed took {0} ms to figure out", sw.ElapsedMilliseconds);
        }
    }
}
