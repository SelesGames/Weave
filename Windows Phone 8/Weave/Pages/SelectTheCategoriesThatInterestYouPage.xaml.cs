﻿using Microsoft.Phone.Controls;
using SelesGames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Weave.FeedLibrary;

namespace weave
{
    public partial class SelectTheCategoriesThatInterestYouPage : PhoneApplicationPage
    {
        public class Category
        {
            public string Name { get; set; }
            public bool IsEnabled { get; set; }
        }

        BundledLibrary library;
        IEnumerable<Category> categories;

        public SelectTheCategoriesThatInterestYouPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = "welcome to " + AppSettings.Instance.AppName;
            warning.Visibility = Visibility.Collapsed;

            library = ServiceResolver.Get<BundledLibrary>();
            categories = library.Feeds.Get().UniqueCategoryNames().OrderBy(o => o).Select(o => new Category { Name = o }).ToList();

            var permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();
            if (permState.IsFirstTime)
            {
                foreach (var category in categories)
                {
                    category.IsEnabled = false;
                };
            }

            list.ItemsSource = (IList)categories;
            nextButton.Click += (s, e) => OnNextButtonClick();
        }

        async void OnNextButtonClick()
        {
            bool atLeast1Selected = CheckForAtLeast1();
            if (atLeast1Selected)
            {
                var enabledCategories = categories.Where(o => o.IsEnabled).Select(o => o.Name).ToList();
                var feedsToAdd = library.Feeds.Get().Where(o => enabledCategories.Contains(o.Category)).ToList();
                var dal = ServiceResolver.Get<Data.Weave4DataAccessLayer>();
                foreach (var feed in feedsToAdd)
                    await dal.AddCustomFeed(feed);

                // TODO: SHOW SOME PROGRESS BAR OR SOMETHING
                await dal.SaveFeeds();

                GlobalNavigationService.ToPanoramaPage();
            }
            else
                NotifyYouNeed1();
        }

        bool CheckForAtLeast1()
        {
            return categories.Where(o => o.IsEnabled).Count() >= 1;
        }
        
        void NotifyYouNeed1()
        {
            warning.Visibility = Visibility.Visible;
        }
    }
}