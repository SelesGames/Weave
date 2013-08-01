using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace weave
{
    public class CategoryOrLooseFeedViewModel : INotifyPropertyChanged
    {
        #region static media values that come from App.Resources

        static bool areArticleBrushesSet = false, areFontFamiliesSet = false;
        static Brush newArticleBrush, noNewArticleBrush;
        static FontFamily categoryFont, feedFont;

        static void SetArticleBrushes()
        {
            newArticleBrush = Application.Current.Resources["PhoneAccentBrush"] as Brush;
            noNewArticleBrush = Application.Current.Resources["PhoneSubtleBrush"] as Brush;
            areArticleBrushesSet = true;
        }

        static void SetFontFamilies()
        {
            categoryFont = Application.Current.Resources["PhoneFontFamilyBlack"] as FontFamily;
            feedFont = Application.Current.Resources["PhoneFontFamilyNormal"] as FontFamily;
            areFontFamiliesSet = true;
        }

        #endregion




        public string Name { get; set; }
        public Guid FeedId { get; set; }
        public int NewArticleCount { get; set; }
        public string ImageSource { get; set; }
        public CategoryOrFeedType Type { get; set; }

        public string NewArticleCountText
        {
            get
            {
                return NewArticleCount > 0 ? NewArticleCount.ToString() : null;
            }
        }

        public Brush NewArticleCountBrush
        {
            get
            {
                if (!areArticleBrushesSet)
                    SetArticleBrushes();

                return NewArticleCount > 0 ? newArticleBrush : noNewArticleBrush;
            }
        }

        public FontFamily NameFontFamily
        {
            get
            {
                if (!areFontFamiliesSet)
                    SetFontFamilies();

                return Type == CategoryOrFeedType.Category ? categoryFont : feedFont;
            }
        }

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
