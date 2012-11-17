using System;
using Microsoft.Phone.Shell;

namespace Weave.LiveTile.ScheduledAgent
{
    /// <summary>
    /// Provides a convenience method for showing a toast that won't annoy the user outside of their normal
    /// waking hours (as defined by the application)
    /// </summary>
    public static class ToastHelper
    {
        /// <summary>
        /// Shows a toast
        /// </summary>
        /// <param name="title">The title of the toast</param>
        /// <param name="content">The content of the toast</param>
        /// <param name="navigationUri">The URI to navigate to if the toast is clicked</param>
        /// <remarks>The toast won't show outside of normal waking hours (in case it wakes you up!)</remarks>
        public static void ShowToast(string title, string content, Uri navigationUri)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                return;

            var toast = new ShellToast();
            toast.Title = title;
            toast.Content = content;
            if (navigationUri != null)
                toast.NavigationUri = navigationUri;

            toast.Show();
        }
    }
}
