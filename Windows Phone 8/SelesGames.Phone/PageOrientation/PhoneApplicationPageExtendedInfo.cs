using System;
using Microsoft.Phone.Controls;

namespace SelesGames.Phone
{
    internal class PhoneApplicationPageExtendedInfo
    {
        WeakReference page;
        internal PhoneApplicationPage Page
        {
            get { return page.Target as PhoneApplicationPage; }
        }
        internal SupportedPageOrientation OriginalSupportedOrientation { get; private set; }

        internal static PhoneApplicationPageExtendedInfo Create(PhoneApplicationPage page)
        {
            var o = new PhoneApplicationPageExtendedInfo
            {
                OriginalSupportedOrientation = page.SupportedOrientations
            };
            o.page = new WeakReference(page);
            return o;
        }

        internal void SetSupportedOrientation(SupportedPageOrientation orientation)
        {
            if (page.Target is PhoneApplicationPage)
                ((PhoneApplicationPage)page.Target).SupportedOrientations = orientation;
        }

        internal void RestoreOriginalSupportedOrientation()
        {
            if (page.Target is PhoneApplicationPage)
                ((PhoneApplicationPage)page.Target).SupportedOrientations = OriginalSupportedOrientation;
        }

        internal PageOrientation GetPageOrientation()
        {
            if (page.Target is PhoneApplicationPage)
                return ((PhoneApplicationPage)page.Target).Orientation;
            else
                return PageOrientation.None;
        }
    }

    //internal class PhoneApplicationPageExtendedInfo
    //{
    //    internal PhoneApplicationPage Page { get; private set; }
    //    internal SupportedPageOrientation OriginalSupportedOrientation { get; private set; }

    //    internal static PhoneApplicationPageExtendedInfo Create(PhoneApplicationPage page)
    //    {
    //        return new PhoneApplicationPageExtendedInfo
    //        {
    //            OriginalSupportedOrientation = page.SupportedOrientations,
    //            Page = page,
    //        };
    //    }

    //    internal void SetSupportedOrientation(SupportedPageOrientation orientation)
    //    {
    //        Page.SupportedOrientations = orientation;
    //    }

    //    internal void RestoreOriginalSupportedOrientation()
    //    {
    //        Page.SupportedOrientations = OriginalSupportedOrientation;
    //    }

    //    internal PageOrientation GetPageOrientation()
    //    {
    //        return Page.Orientation;
    //    }
    //}
}
