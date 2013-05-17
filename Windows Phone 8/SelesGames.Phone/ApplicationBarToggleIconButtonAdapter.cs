using Microsoft.Phone.Shell;
using System;
using System.Windows;

namespace SelesGames.Phone
{
    public class ApplicationBarToggleIconButtonAdapter : FrameworkElement, IDisposable
    {
        public event EventHandler IsCheckedChanged;




        #region Dependency Properties




        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(ApplicationBarToggleIconButtonAdapter), new PropertyMetadata(true, OnEnabledChanged));

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleIconButtonAdapter)d).Item.IsEnabled = (bool)e.NewValue;
        }

        #endregion




        #region CheckedTextProperty

        public static readonly DependencyProperty CheckedTextProperty = DependencyProperty.RegisterAttached(
            "CheckedText", typeof(string), typeof(ApplicationBarToggleIconButtonAdapter), new PropertyMetadata(OnTextChanged));

        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }

        static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleIconButtonAdapter)d).UpdateDisplay();
        }

        #endregion




        #region UncheckedTextProperty

        public static readonly DependencyProperty UncheckedTextProperty = DependencyProperty.RegisterAttached(
            "UncheckedText", typeof(string), typeof(ApplicationBarToggleIconButtonAdapter), new PropertyMetadata(OnOffTextChanged));

        public string UncheckedText
        {
            get { return (string)GetValue(UncheckedTextProperty); }
            set { SetValue(UncheckedTextProperty, value); }
        }

        static void OnOffTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleIconButtonAdapter)d).UpdateDisplay();
        }

        #endregion




        #region CheckedIconUriProperty

        public static readonly DependencyProperty CheckedIconUriProperty = DependencyProperty.RegisterAttached(
            "CheckedIconUri", typeof(Uri), typeof(ApplicationBarToggleIconButtonAdapter), new PropertyMetadata(OnCheckedIconUriChanged));

        public Uri CheckedIconUri
        {
            get { return (Uri)GetValue(CheckedIconUriProperty); }
            set { SetValue(CheckedIconUriProperty, value); }
        }

        static void OnCheckedIconUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleIconButtonAdapter)d).UpdateDisplay();
        }

        #endregion




        #region UncheckedIconUriProperty

        public static readonly DependencyProperty UncheckedIconUriProperty = DependencyProperty.RegisterAttached(
            "UncheckedIconUri", typeof(Uri), typeof(ApplicationBarToggleIconButtonAdapter), new PropertyMetadata(OnUncheckedIconUriChanged));

        public Uri UncheckedIconUri
        {
            get { return (Uri)GetValue(UncheckedIconUriProperty); }
            set { SetValue(UncheckedIconUriProperty, value); }
        }

        static void OnUncheckedIconUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleIconButtonAdapter)d).UpdateDisplay();
        }

        #endregion




        #region IsChecked

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached(
            "IsChecked", typeof(bool), typeof(ApplicationBarToggleIconButtonAdapter), new PropertyMetadata(false, OnIsCheckedChanged));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var adapter = (ApplicationBarToggleIconButtonAdapter)d;
                adapter.UpdateDisplay();
                if (adapter.IsCheckedChanged != null)
                    adapter.IsCheckedChanged(adapter, EventArgs.Empty);
            }
        }

        #endregion




        #endregion




        void UpdateDisplay()
        {
            if (Item == null)
                return;

            if (IsChecked)
            {
                Item.Text = CheckedText ?? "on";
                Item.IconUri = CheckedIconUri;
            }
            else
            {
                Item.Text = UncheckedText ?? "off";
                Item.IconUri = UncheckedIconUri;
            }
        }


        public IApplicationBarIconButton Item { get; private set; }

        public ApplicationBarToggleIconButtonAdapter(IApplicationBarIconButton item)
        {
            CheckedText = item.Text;
            CheckedIconUri = item.IconUri;
            UncheckedIconUri = item.IconUri;
            Item = item;
            Item.Click += item_Click;
        }

        void item_Click(object sender, EventArgs e)
        {
            IsChecked = !IsChecked;
        }

        public void Dispose()
        {
            Item.Click -= item_Click;
        }
    }
}
