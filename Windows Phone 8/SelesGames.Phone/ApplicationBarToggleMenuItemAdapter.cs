﻿using Microsoft.Phone.Shell;
using System;
using System.Windows;

namespace SelesGames.Phone
{
    public class ApplicationBarToggleMenuItemAdapter : FrameworkElement, IDisposable
    {
        #region Dependency Properties

        
        
        
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(ApplicationBarToggleMenuItemAdapter), new PropertyMetadata(true, OnEnabledChanged));

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleMenuItemAdapter)d).Item.IsEnabled = (bool)e.NewValue;
        }

        #endregion




        #region TextProperty

        public static readonly DependencyProperty CheckedTextProperty = DependencyProperty.RegisterAttached(
            "CheckedText", typeof(string), typeof(ApplicationBarToggleMenuItemAdapter), new PropertyMetadata(OnTextChanged));

        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleMenuItemAdapter)d).UpdateDisplay();
        }

        #endregion




        #region OffTextProperty

        public static readonly DependencyProperty UncheckedTextProperty = DependencyProperty.RegisterAttached(
            "UncheckedText", typeof(string), typeof(ApplicationBarToggleMenuItemAdapter), new PropertyMetadata(OnOffTextChanged));

        public string UncheckedText
        {
            get { return (string)GetValue(UncheckedTextProperty); }
            set { SetValue(UncheckedTextProperty, value); }
        }

        private static void OnOffTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleMenuItemAdapter)d).UpdateDisplay();
        }

        #endregion




        #region IsChecked

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached(
            "IsChecked", typeof(bool), typeof(ApplicationBarToggleMenuItemAdapter), new PropertyMetadata(false, OnIsCheckedChanged));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((ApplicationBarToggleMenuItemAdapter)d).UpdateDisplay();
        }

        #endregion




        #endregion




        void UpdateDisplay()
        {
            if (Item == null)
                return;

            if (IsChecked)
                Item.Text = CheckedText ?? "on";
            else
                Item.Text = UncheckedText ?? "off";
        }


        public IApplicationBarMenuItem Item { get; private set; }

        public ApplicationBarToggleMenuItemAdapter(IApplicationBarMenuItem item)
        {
            CheckedText = item.Text;
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
