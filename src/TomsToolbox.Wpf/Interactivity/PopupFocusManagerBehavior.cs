﻿namespace TomsToolbox.Wpf.Interactivity
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    using JetBrains.Annotations;

    using Microsoft.Xaml.Behaviors;

    /// <summary>
    /// Handle focus for popups opened by toggle buttons. 
    /// When the popup opens, the focus is set to the first focusable control in the popup.
    /// When the popup closes, the focus is set back to the button.
    /// </summary>
    public class PopupFocusManagerBehavior : Behavior<Popup>
    {
        /// <summary>
        /// Gets or sets the toggle button that controls the popup.
        /// </summary>
        [CanBeNull]
        public ToggleButton? ToggleButton
        {
            get => (ToggleButton)GetValue(ToggleButtonProperty);
            set => SetValue(ToggleButtonProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="ToggleButton"/> dependency property.
        /// </summary>
        [NotNull] public static readonly DependencyProperty ToggleButtonProperty =
            DependencyProperty.Register("ToggleButton", typeof(ToggleButton), typeof(PopupFocusManagerBehavior));

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            var popup = AssociatedObject;

            popup.IsKeyboardFocusWithinChanged += Popup_IsKeyboardFocusWithinChanged;
            popup.Opened += Popup_Opened;
            popup.KeyDown += Popup_KeyDown;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>
        /// Override this to unhook functionality from the AssociatedObject.
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            var popup = AssociatedObject;

            popup.IsKeyboardFocusWithinChanged -= Popup_IsKeyboardFocusWithinChanged;
            popup.Opened -= Popup_Opened;
            popup.KeyDown -= Popup_KeyDown;
        }

        private void Popup_KeyDown([CanBeNull] object? sender, [CanBeNull] KeyEventArgs? e)
        {
            if (ToggleButton == null)
                return;

            switch (e?.Key)
            {
                case Key.Escape:
                case Key.Enter:
                case Key.Tab:
                    ToggleButton.Focus();
                    break;
            }
        }

        private void Popup_Opened([CanBeNull] object? sender, [CanBeNull] EventArgs? e)
        {
            if (!(sender is Popup popup))
                return;

            var child = popup.Child;
            var focusable = child?.VisualDescendantsAndSelf().OfType<UIElement>().FirstOrDefault(item => item.Focusable);
            if (focusable != null)
            {
                popup.BeginInvoke(() => focusable.Focus());
            }
        }

        private void Popup_IsKeyboardFocusWithinChanged([CanBeNull] object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (Equals(e.NewValue, false) && (ToggleButton != null))
            {
                ToggleButton.IsChecked = false;
            }
        }
    }
}
