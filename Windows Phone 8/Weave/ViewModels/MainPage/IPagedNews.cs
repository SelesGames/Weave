using System;
using System.Collections.Generic;
using Weave.ViewModels;

namespace Weave.WP.ViewModels.MainPage
{
    public interface IPagedNews
    {
        event EventHandler CountChanged;

        int PageSize { get; }
        int NumberOfPagesToTakeAtATime { get; }
        int PageCount { get; }
        int TotalNewsCount { get; }
        int NewNewsCount { get; }

        IEnumerable<AsyncNewsList> GetNewsLists(EntryType initialEntryType);
        //AsyncNewsList GetPage(int desiredPage, EntryType initialEntryType);
    }
}