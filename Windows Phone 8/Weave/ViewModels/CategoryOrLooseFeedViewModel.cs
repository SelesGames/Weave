using System;
using System.ComponentModel;

namespace weave
{
    public class CategoryOrLooseFeedViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Guid FeedId { get; set; }
        public string NewCount { get; set; }
        public string Source { get; set; }
        public CategoryOrFeedType Type { get; set; }

        //public async Task UpdateNewsCountAfterRefreshAsync()
        //{
        //    RefreshListenerBase refreshListener;

        //    if (Type == CategoryOrFeedType.Category)
        //        refreshListener = new CategoryRefreshListener(Name);
        //    else if (Type == CategoryOrFeedType.Feed)
        //        refreshListener = new FeedRefreshListener(FeedId);
        //    else
        //        throw new Exception("unexpected value for CategoryOrFeedType");

        //    var refreshed = refreshListener.GetRefreshed();

        //    if (!refreshed.IsCompleted)
        //    {
        //        // show progress bar
        //        await refreshed;
        //        // hide progress bar
        //    }
        //    var newCount = refreshListener.GetNewCount();
        //    if (newCount > 0)
        //        NewCount = newCount.ToString();
        //    else
        //        NewCount = null;
        //    PropertyChanged.Raise(this, "NewCount");
        //}

        public override bool Equals(object obj)
        {
            if (Name == null)
                return false;

            if (obj is CategoryOrLooseFeedViewModel)
            {
                var vm = (CategoryOrLooseFeedViewModel)obj;

                // if they are both Categories, compare them by name
                if (Type == CategoryOrFeedType.Category && vm.Type == CategoryOrFeedType.Category)
                {
                    return Name.Equals(vm.Name) && Type.Equals(vm.Type);
                }
                // if they are both Feeds, compare them by FeedId
                else if (Type == CategoryOrFeedType.Feed && vm.Type == CategoryOrFeedType.Feed)
                {
                    return FeedId.Equals(vm.FeedId);
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name == null ? -1 : Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name.ToString();
        }

        public enum CategoryOrFeedType
        {
            Category,
            Feed
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
