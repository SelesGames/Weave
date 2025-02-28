﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Microsoft.Phone.Controls.Primitives;
using System;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    abstract class DataSource : ILoopingSelectorDataSource
    {
        private DateTimeWrapper _selectedItem;

        public object GetNext(object relativeTo)
        {
            DateTime? next = GetRelativeTo(((DateTimeWrapper)relativeTo).DateTime, 1);
            return next.HasValue ? new DateTimeWrapper(next.Value) : null;
        }

        public object GetPrevious(object relativeTo)
        {
            DateTime? next = GetRelativeTo(((DateTimeWrapper)relativeTo).DateTime, -1);
            return next.HasValue ? new DateTimeWrapper(next.Value) : null;
        }

        protected abstract DateTime? GetRelativeTo(DateTime relativeDate, int delta);

        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != _selectedItem)
                {
                    DateTimeWrapper valueWrapper = (DateTimeWrapper)value;
                    if ((null == valueWrapper) || (null == _selectedItem) || (valueWrapper.DateTime != _selectedItem.DateTime))
                    {
                        object previousSelectedItem = _selectedItem;
                        _selectedItem = valueWrapper;
                        var handler = SelectionChanged;
                        if (null != handler)
                        {
                            handler(this, new SelectionChangedEventArgs(new object[] { previousSelectedItem }, new object[] { _selectedItem }));
                        }
                    }
                }
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
    }

    class YearDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            if ((1601 == relativeDate.Year) || (3000 == relativeDate.Year))
            {
                return null;
            }
            int nextYear = relativeDate.Year + delta;
            int nextDay = Math.Min(relativeDate.Day, DateTime.DaysInMonth(nextYear, relativeDate.Month));
            return new DateTime(nextYear, relativeDate.Month, nextDay, relativeDate.Hour, relativeDate.Minute, relativeDate.Second);
        }
    }

    class MonthDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int monthsInYear = 12;
            int nextMonth = ((monthsInYear + relativeDate.Month - 1 + delta) % monthsInYear) + 1;
            int nextDay = Math.Min(relativeDate.Day, DateTime.DaysInMonth(relativeDate.Year, nextMonth));
            return new DateTime(relativeDate.Year, nextMonth, nextDay, relativeDate.Hour, relativeDate.Minute, relativeDate.Second);
        }
    }

    class DayDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int daysInMonth = DateTime.DaysInMonth(relativeDate.Year, relativeDate.Month);
            int nextDay = ((daysInMonth + relativeDate.Day - 1 + delta) % daysInMonth) + 1;
            return new DateTime(relativeDate.Year, relativeDate.Month, nextDay, relativeDate.Hour, relativeDate.Minute, relativeDate.Second);
        }
    }

    class TwelveHourDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int hoursInHalfDay = 12;
            int nextHour = (hoursInHalfDay + relativeDate.Hour + delta) % hoursInHalfDay;
            nextHour += hoursInHalfDay <= relativeDate.Hour ? hoursInHalfDay : 0;
            return new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, nextHour, relativeDate.Minute, 0);
        }
    }

    class MinuteDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int minutesInHour = 60;
            int nextMinute = (minutesInHour + relativeDate.Minute + delta) % minutesInHour;
            return new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, relativeDate.Hour, nextMinute, 0);
        }
    }

    class AmPmDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int hoursInDay = 24;
            int nextHour = relativeDate.Hour + (delta * (hoursInDay / 2));
            if ((nextHour < 0) || (hoursInDay <= nextHour))
            {
                return null;
            }
            return new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, nextHour, relativeDate.Minute, 0);
        }
    }

    class TwentyFourHourDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int hoursInDay = 24;
            int nextHour = (hoursInDay + relativeDate.Hour + delta) % hoursInDay;
            return new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, nextHour, relativeDate.Minute, 0);
        }
    }
}
