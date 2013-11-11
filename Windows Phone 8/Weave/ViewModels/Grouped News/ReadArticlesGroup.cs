﻿using System.Threading.Tasks;
using Weave.ViewModels;

namespace weave
{
    public class ReadArticlesGroup : NewsItemGroup
    {
        UserInfo user;

        public ReadArticlesGroup(UserInfo user)
        {
            this.user = user;

            DisplayName = "Previously Read";
            NewArticleCount = -1;
            UnreadArticleCount = -1;
            TotalArticleCount = 9999;
            FeedCount = -1;
        }

        public async override Task<NewsList> GetNewsList(EntryType entryType, int skip, int take)
        {
            var favorites = await user.GetRead(skip, take);

            var newsList = new NewsList
            {
                FeedCount = -1,
                NewArticleCount = -1,
                UnreadArticleCount = -1,
                TotalArticleCount = 9999,
                IncludedArticleCount = favorites.Count,
                Skip = skip,
                Take = take,
                News = favorites,
            };

            return newsList;
        }


        public override string GetTeaserPicImageUrl()
        {
            return null;
        }

        protected override void OnMarkEntry() { }

        protected override Microsoft.Phone.Shell.ShellTile GetShellTile()
        {
            return null;
        }
    }
}
