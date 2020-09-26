﻿namespace TomsToolbox.Wpf.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using JetBrains.Annotations;

    /// <summary>
    /// A loading indicator showing a rotating animation when active.
    /// </summary>
    public class LoadingIndicator : Control
    {
        static LoadingIndicator()
        {
            DefaultStyleKeyProperty?.OverrideMetadata(typeof(LoadingIndicator), new FrameworkPropertyMetadata(typeof(LoadingIndicator)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingIndicator"/> class.
        /// </summary>
        public LoadingIndicator()
        {
            Focusable = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the loading indicator is active or hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get => this.GetValue<bool>(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property
        /// </summary>
        [NotNull] public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(LoadingIndicator));


        /// <summary>
        /// Gets or sets the header that is displayed centered above the graphics.
        /// </summary>
        [CanBeNull]
        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property
        /// </summary>
        [NotNull] public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(LoadingIndicator));


        /// <summary>
        /// Gets or sets the background when the loading indicator is active.
        /// </summary>
        public Color ActiveBackgroundColor
        {
            get => this.GetValue<Color>(ActiveBackgroundColorProperty);
            set => SetValue(ActiveBackgroundColorProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="ActiveBackgroundColor"/> dependency property
        /// </summary>
        [NotNull] public static readonly DependencyProperty ActiveBackgroundColorProperty =
            DependencyProperty.Register("ActiveBackgroundColor", typeof(Color), typeof(LoadingIndicator), new FrameworkPropertyMetadata(Colors.Transparent));


        /// <summary>
        /// Gets or sets the layout transform applied to the animation.
        /// </summary>
        [CanBeNull]
        public Transform? AnimationLayoutTransform
        {
            get => (Transform)GetValue(AnimationLayoutTransformProperty);
            set => SetValue(AnimationLayoutTransformProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="AnimationLayoutTransform"/> dependency property
        /// </summary>
        [NotNull] public static readonly DependencyProperty AnimationLayoutTransformProperty =
            DependencyProperty.Register("AnimationLayoutTransform", typeof(Transform), typeof(LoadingIndicator));
    }
}
