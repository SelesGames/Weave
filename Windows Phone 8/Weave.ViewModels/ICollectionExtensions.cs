﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.ViewModels
{
    internal static class ICollectionExtensions
    {
        public static void OrderedUniqueInsert<T, TOrder>(this IList<T> sourceList,
            IList<T> insertItems,
            Func<T, TOrder> orderKeySelector,
            IEqualityComparer<T> equalityComparer = null)

            where TOrder : IComparable
        {
            var newEntryIndex = 0;

            IList<T> proper;

            if (equalityComparer == null)
                proper = insertItems.Except(sourceList).OrderBy(orderKeySelector).ToList();
            else
                proper = insertItems.Except(sourceList, equalityComparer).OrderBy(orderKeySelector).ToList();

            for (int i = 0; i < sourceList.Count; i++)
            {
                if (newEntryIndex >= proper.Count)
                    break;

                var current = proper[newEntryIndex];
                var newsItem = sourceList[i];

                if (orderKeySelector(current).CompareTo(orderKeySelector(newsItem)) < 0)
                {
                    sourceList.Insert(i, current);
                    newEntryIndex++;
                }
            }
        }

        public static void OrderedDescendingUniqueInsert<T, TOrder>(this IList<T> sourceList,
            IList<T> insertItems,
            Func<T, TOrder> orderKeySelector,
            IEqualityComparer<T> equalityComparer = null)

            where TOrder : IComparable
        {
            var newEntryIndex = 0;

            IList<T> proper;

            if (equalityComparer == null)
                proper = insertItems.Except(sourceList).OrderByDescending(orderKeySelector).ToList();
            else
                proper = insertItems.Except(sourceList, equalityComparer).OrderByDescending(orderKeySelector).ToList();

            for (int i = 0; i < sourceList.Count; i++)
            {
                if (newEntryIndex >= proper.Count)
                    break;

                var current = proper[newEntryIndex];
                var newsItem = sourceList[i];

                if (orderKeySelector(current).CompareTo(orderKeySelector(newsItem)) > 0)
                {
                    sourceList.Insert(i, current);
                    newEntryIndex++;
                }
            }
        }
    }
}
