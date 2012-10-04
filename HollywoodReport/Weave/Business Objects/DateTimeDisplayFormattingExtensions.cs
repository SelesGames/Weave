using System;

namespace weave
{
    public static class TwitterDateTimeExtension
    {
        public static string ElapsedTime(this DateTime time)
        {
            DateTime relativeTo = DateTime.Now;

            // time difference in seconds
            double delta = relativeTo.Subtract(time).TotalSeconds;

            /*if (delta < 60)
            {
                //return "less than a minute ago";
                return "under a minute ago";
            }
            else */
            if (delta < 120)
            {
                //return "about a minute ago";
                return "1 minute ago";
            }
            else if (delta < (60 * 60))
            {
                return (int)(delta / 60) + " minutes ago";
            }
            else if (delta < (120 * 60))
            {
                //return "about an hour ago";
                return "1 hour ago";
            }
            else if (delta < (24 * 60 * 60))
            {
                //return string.Format("about {0} hours ago", (int)(delta / 3600));
                return string.Format("{0} hours ago", (int)(delta / 3600));
            }
            else if (delta < (48 * 60 * 60))
            {
                return "1 day ago";
            }
            else
            {
                return (int)(delta / 86400) + " days ago";
            }
        }
    }
}
