﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a selectable item inside a Menu or ContextMenu.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MenuItem))]
    public class MenuItem : HeaderedItemsControl // , ICommandSource // ICommandSource not defined by Silverlight 4
    {
        /// <summary>
        /// Occurs when a MenuItem is clicked.
        /// </summary>
        public event RoutedEventHandler Click;

        /// <summary>
        /// Stores a value indicating whether this element has logical focus.
        /// </summary>
        private bool _isFocused;

        /// <summary>
        /// Gets or sets a reference to the MenuBase parent.
        /// </summary>
        internal MenuBase ParentMenuBase { get; set; }

        /// <summary>
        /// Gets or sets the command associated with the menu item.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Identifies the Command dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(MenuItem),
            new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Handles changes to the Command DependencyProperty.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnCommandChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)o).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the Command property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        private void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            if (null != oldValue)
            {
                oldValue.CanExecuteChanged -= OnCanExecuteChanged;
            }
            if (null != newValue)
            {
                newValue.CanExecuteChanged += OnCanExecuteChanged;
            }
            UpdateIsEnabled(true);
        }

        /// <summary>
        /// Gets or sets the parameter to pass to the Command property of a MenuItem.
        /// </summary>
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Identifies the CommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(MenuItem),
            new PropertyMetadata(null, OnCommandParameterChanged));

        /// <summary>
        /// Handles changes to the CommandParameter DependencyProperty.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnCommandParameterChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)o).OnCommandParameterChanged(/*e.OldValue, e.NewValue*/);
        }

        /// <summary>
        /// Handles changes to the CommandParameter property.
        /// </summary>
        private void OnCommandParameterChanged(/*object oldValue, object newValue*/)
        {
            UpdateIsEnabled(true);
        }

        /// <summary>
        /// Initializes a new instance of the MenuItem class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "False parameter to UpdateIsEnabled ensures ChangeVisualState won't be called.")]
        public MenuItem()
        {
            DefaultStyleKey = typeof(MenuItem);
            IsEnabledChanged += OnIsEnabledChanged;
            SetValue(TiltEffect.IsTiltEnabledProperty, true);
            Loaded += OnLoaded;
            UpdateIsEnabled(false);
        }

        /// <summary>
        /// Called when the template's tree is generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ChangeVisualState(false);
        }

        /// <summary>
        /// Invoked whenever an unhandled GotFocus event reaches this element in its route.
        /// </summary>
        /// <param name="e">A RoutedEventArgs that contains event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _isFocused = true;
            ChangeVisualState(true);
        }

        /// <summary>
        /// Raises the LostFocus routed event by using the event data that is provided.
        /// </summary>
        /// <param name="e">A RoutedEventArgs that contains event data.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            _isFocused = false;
            ChangeVisualState(true);
        }

        /// <summary>
        /// Called whenever the mouse enters a MenuItem.
        /// </summary>
        /// <param name="e">The event data for the MouseEnter event.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
            ChangeVisualState(true);
        }

        /// <summary>
        /// Called whenever the mouse leaves a MenuItem.
        /// </summary>
        /// <param name="e">The event data for the MouseLeave event.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (null != ParentMenuBase)
            {
                ParentMenuBase.Focus();
            }
            ChangeVisualState(true);
        }

        /// <summary>
        /// Called when the left mouse button is released.
        /// </summary>
        /// <param name="e">The event data for the MouseLeftButtonUp event.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (!e.Handled)
            {
                OnClick();
                e.Handled = true;
            }

            base.OnMouseLeftButtonUp(e);
        }

        /// <summary>
        /// Responds to the KeyDown event.
        /// </summary>
        /// <param name="e">The event data for the KeyDown event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (!e.Handled && (Key.Enter == e.Key))
            {
                OnClick();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Called when the Items property changes.
        /// </summary>
        /// <param name="e">The event data for the ItemsChanged event.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when a MenuItem is clicked and raises a Click event.
        /// </summary>
        protected virtual void OnClick()
        {
            ContextMenu contextMenu = ParentMenuBase as ContextMenu;
            if (null != contextMenu)
            {
                contextMenu.ChildMenuItemClicked();
            }
            // Wrapping the remaining code in a call to Dispatcher.BeginInvoke provides
            // WPF-compatibility by allowing the ContextMenu to close before the command
            // executes. However, it breaks the Clipboard.SetText scenario because the
            // call to SetText is no longer in direct response to user input.
            var handler = Click;
            if (null != handler)
            {
                handler(this, new RoutedEventArgs());
            }
            if ((null != Command) && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        /// <summary>
        /// Handles the CanExecuteChanged event of the Command property.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateIsEnabled(true);
        }

        /// <summary>
        /// Updates the IsEnabled property.
        /// </summary>
        /// <remarks>
        /// WPF overrides the local value of IsEnabled according to ICommand, so Silverlight does, too.
        /// </remarks>
        /// <param name="changeVisualState">True if ChangeVisualState should be called.</param>
        private void UpdateIsEnabled(bool changeVisualState)
        {
            IsEnabled = (null == Command) || Command.CanExecute(CommandParameter);
            if (changeVisualState)
            {
                ChangeVisualState(true);
            }
        }

        /// <summary>
        /// Called when the IsEnabled property changes.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChangeVisualState(true);
        }

        /// <summary>
        /// Called when the Loaded event is raised.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ChangeVisualState(false);
        }

        /// <summary>
        /// Changes to the correct visual state(s) for the control.
        /// </summary>
        /// <param name="useTransitions">True to use transitions; otherwise false.</param>
        protected virtual void ChangeVisualState(bool useTransitions)
        {
            if (!IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }

            if (_isFocused && IsEnabled)
            {
                VisualStateManager.GoToState(this, "Focused", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Unfocused", useTransitions);
            }
        }
    }
}
