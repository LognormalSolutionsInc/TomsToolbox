﻿namespace TomsToolbox.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;

    /// <summary>
    /// Extensions methods to ease dealing with dependency objects.
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Gets the window handle of the HwndSource hosting this item.
        /// </summary>
        /// <param name="self">The item.</param>
        /// <returns>The window handle, if the item is part of a valid visual tree, otherwise IntPtr.Zero.</returns>
        public static IntPtr GetWindowHandle(this DependencyObject self)
        {
            Contract.Requires(self != null);

            var hwndSource = PresentationSource.FromDependencyObject(self) as HwndSource;
            return (hwndSource != null) ? hwndSource.Handle : IntPtr.Zero;
        }

        /// <summary>
        /// Gets the root visual for the item.
        /// </summary>
        /// <param name="item">The item to find the root visual for.</param>
        /// <returns>The root visual.</returns>
        /// <exception cref="ArgumentException">The item is not part of a valid visual tree.</exception>
        /// <remarks>
        /// If the item is inside a control that's embedded in a native or WindowsForms window, the root visual
        /// is <c>not</c> a <see cref="System.Windows.Window"/>.
        /// </remarks>
        public static FrameworkElement GetRootVisual(this DependencyObject item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<FrameworkElement>() != null);

            var hwndSource = (HwndSource)PresentationSource.FromDependencyObject(item);
            if (hwndSource == null)
            {
                throw new ArgumentException(@"Item not part of a valid visual tree.", "item");
            }
            var compositionTarget = hwndSource.CompositionTarget;
            if (compositionTarget == null)
            {
                throw new ArgumentException(@"Item not part of a valid visual tree.", "item");
            }

            var rootVisual = (FrameworkElement)compositionTarget.RootVisual;
            if (rootVisual == null)
            {
                throw new ArgumentException(@"Item not part of a valid visual tree.", "item");
            }

            return rootVisual;
        }

        /// <summary>
        /// Gets the root visual for the item.
        /// </summary>
        /// <param name="item">The item to find the root visual for.</param>
        /// <returns>The root visual if the item is part of a valid visual tree; otherwise <c>null</c>.
        /// </returns>
        /// /// <remarks>
        /// If the item is inside a control that's embedded in a native or WindowsForms window, the root visual
        /// is <c>not</c> a <see cref="System.Windows.Window"/>.
        /// </remarks>
        public static FrameworkElement TryGetRootVisual(this DependencyObject item)
        {
            Contract.Requires(item != null);

            var hwndSource = (HwndSource)PresentationSource.FromDependencyObject(item);
            if (hwndSource == null)
            {
                return null;
            }
            var compositionTarget = hwndSource.CompositionTarget;
            if (compositionTarget == null)
            {
                return null;
            }

            var rootVisual = (FrameworkElement)compositionTarget.RootVisual;

            return rootVisual;
        }

        /// <summary>
        /// Returns an enumeration of elements that contain this element, and the ancestors of this element.
        /// </summary>
        /// <param name="self">The starting element.</param>
        /// <returns>The ancestor list.</returns>
        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject self)
        {
            Contract.Requires(self != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);

            while (self != null)
            {
                yield return self;
                self = LogicalTreeHelper.GetParent(self) ?? VisualTreeHelper.GetParent(self);
            }
        }

        /// <summary>
        /// Returns an enumeration of the ancestor elements of this element.
        /// </summary>
        /// <param name="self">The starting element.</param>
        /// <returns>The ancestor list.</returns>
        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject self)
        {
            Contract.Requires(self != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);

            return self.AncestorsAndSelf().Skip(1);
        }

        /// <summary>
        /// Returns the first element in the ancestor list that implements the type of the type parameter.
        /// </summary>
        /// <typeparam name="T">The type of element to return.</typeparam>
        /// <param name="self">The starting element.</param>
        /// <returns>The first element matching the criteria, or null if no element was found.</returns>
        public static T TryFindAncestorOrSelf<T>(this DependencyObject self)
        {
            Contract.Requires(self != null);

            return self.AncestorsAndSelf().OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Returns the first element in the ancestor list that implements the type of the type parameter.
        /// </summary>
        /// <typeparam name="T">The type of element to return.</typeparam>
        /// <param name="self">The starting element.</param>
        /// <param name="match">The predicate to match.</param>
        /// <returns>The first element matching the criteria, or null if no element was found.</returns>
        public static T TryFindAncestorOrSelf<T>(this DependencyObject self, Func<T, bool> match)
        {
            Contract.Requires(self != null);
            Contract.Requires(match != null);

            return self.AncestorsAndSelf().OfType<T>().FirstOrDefault(match);
        }

        /// <summary>
        /// Returns the first element in the ancestor list that implements the type of the type parameter.
        /// </summary>
        /// <typeparam name="T">The type of element to return.</typeparam>
        /// <param name="self">The starting element.</param>
        /// <returns>The first element matching the criteria, or null if no element was found.</returns>
        public static T TryFindAncestor<T>(this DependencyObject self)
        {
            Contract.Requires(self != null);

            return self.Ancestors().OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Returns the first element in the ancestor list that implements the type of the type parameter.
        /// </summary>
        /// <typeparam name="T">The type of element to return.</typeparam>
        /// <param name="self">The starting element.</param>
        /// <param name="match">The predicate to match.</param>
        /// <returns>The first element matching the criteria, or null if no element was found.</returns>
        public static T TryFindAncestor<T>(this DependencyObject self, Func<T, bool> match)
        {
            Contract.Requires(self != null);
            Contract.Requires(match != null);

            return self.Ancestors().OfType<T>().FirstOrDefault(match);
        }

        /// <summary>
        /// Enumerates the immediate children of the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The immediate children of the specified item.</returns>
        /// <remarks>
        /// Uses <see cref="VisualTreeHelper.GetChildrenCount"/> and <see cref="VisualTreeHelper.GetChild"/>.
        /// </remarks>
        public static IEnumerable<DependencyObject> VisualChildren(this DependencyObject item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);

            var numberOfChildren = VisualTreeHelper.GetChildrenCount(item);
            for (var i = 0; i < numberOfChildren; i++)
            {
                var child = VisualTreeHelper.GetChild(item, i);
                if (child == null)
                    continue;

                yield return child;
            }
        }

        /// <summary>
        /// Enumerates the specified item and it's immediate children.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The specified item and it's immediate.</returns>
        /// <remarks>
        /// Uses <see cref="VisualTreeHelper.GetChildrenCount"/> and <see cref="VisualTreeHelper.GetChild"/>.
        /// </remarks>
        public static IEnumerable<DependencyObject> VisualChildrenAndSelf(this DependencyObject item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);

            yield return item;

            foreach (var x in item.VisualChildren())
            {
                yield return x;
            }
        }

        /// <summary>
        /// Enumerates all visuals descendants of the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The descendants of the item.</returns>
        /// <remarks>
        /// Uses <see cref="VisualTreeHelper.GetChildrenCount"/> and <see cref="VisualTreeHelper.GetChild"/>.
        /// </remarks>
        public static IEnumerable<DependencyObject> VisualDescendants(this DependencyObject item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);

            foreach (var child in item.VisualChildren())
            {
                yield return child;

                foreach (var x in child.VisualDescendants())
                {
                    yield return x;
                }
            }
        }

        /// <summary>
        /// Enumerates the specified item and all it's visual descendants.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The specified item and all it's visual descendants.</returns>
        /// <remarks>
        /// Uses <see cref="VisualTreeHelper.GetChildrenCount"/> and <see cref="VisualTreeHelper.GetChild"/>.
        /// </remarks>
        public static IEnumerable<DependencyObject> VisualDescendantsAndSelf(this DependencyObject item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);

            yield return item;

            foreach (var x in item.VisualDescendants())
            {
                yield return x;
            }
        }

        /// <summary>
        /// Gets the extent of the thickness when applied to an empty rectangle.
        /// </summary>
        /// <param name="value">The thickness.</param>
        /// <returns>The extent of the thickness.</returns>
        /// <remarks>
        /// Returns a <see cref="Vector"/> because <see cref="Thickness"/> allows negative values.
        /// </remarks>
        public static Vector GetExtent(this Thickness value)
        {
            return new Vector(value.Left + value.Right, value.Top + value.Bottom);
        }
    }
}