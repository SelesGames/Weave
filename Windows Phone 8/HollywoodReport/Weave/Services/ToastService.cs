using System.Windows;
using ToastPromptControl;

namespace weave
{
    public static class ToastService
    {
        public static void ToastPrompt(string message)
        {
            new ToastPrompt
            {
                MillisecondsUntilHidden = 3000,
                Message = message,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeMediumLarge"],
                //TextOrientation = System.Windows.Controls.Orientation.Vertical
            }
            .Show();
        }
    }
}
