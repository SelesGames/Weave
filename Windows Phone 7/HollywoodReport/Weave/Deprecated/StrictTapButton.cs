using System;
using System.Windows;
using System.Windows.Controls;

namespace weave
{
    public class StrictTapButton : Button
    {
        public new event RoutedEventHandler Click;

        public StrictTapButton()
        {
            DefaultStyleKey = typeof(ContentControl);
            this.GetManipulationTap(int.MaxValue).Subscribe(notUsed => this.Click.Raise(this, new RoutedEventArgs()));
        }       
    }
}
