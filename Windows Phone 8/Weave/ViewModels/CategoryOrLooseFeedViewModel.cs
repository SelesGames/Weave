using System;
using System.ComponentModel;

namespace weave
{
    public class CategoryOrLooseFeedViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Guid FeedId { get; set; }
        public string NewArticleCount { get; set; }
        public string Source { get; set; }
        public CategoryOrFeedType Type { get; set; }



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
