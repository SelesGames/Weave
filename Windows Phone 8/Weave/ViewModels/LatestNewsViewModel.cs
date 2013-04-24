using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Weave.ViewModels;

namespace weave
{
    public class LatestNewsViewModel : INotifyPropertyChanged
    {
        IEnumerable<NewsItem> previousNewsItems = new List<NewsItem>();
        IEnumerable<NewsItem> latestNews;

        public IEnumerable<NewsItem> LatestNews 
        {
            get { return latestNews; }
            set
            {
                latestNews = value;
                DetermineIfNewsIsChanged();
            }
        }

        public void DetermineIfNewsIsChanged()
        {
            bool areItemsNew = !Enumerable.SequenceEqual(latestNews, previousNewsItems);

            if (areItemsNew)
            {
                previousNewsItems = latestNews;
                GlobalDispatcher.Current.BeginInvoke(() => PropertyChanged.Raise(this, "LatestNews"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}