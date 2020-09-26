﻿namespace TomsToolbox.Wpf.Composition
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using JetBrains.Annotations;

    using TomsToolbox.Wpf.Composition.XamlExtensions;

    /// <summary>
    /// Retrieves the exported object that matches RegionId and Role from the composition container
    /// and assigns it as the content of the associated <see cref="ContentControl"/>
    /// </summary>
    public class ContentControlCompositionBehavior : VisualCompositionBehavior<ContentControl>
    {
        /// <summary>
        /// Gets or sets the name of the item that should be attached.
        /// </summary>
        /// <remarks>
        /// The first exported item matching RegionId and Role will be set as the content of the content control.
        /// </remarks>
        [CanBeNull]
        public object? Role
        {
            get => GetValue(RoleProperty);
            set => SetValue(RoleProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="Role"/> dependency property.
        /// </summary>
        [NotNull] public static readonly DependencyProperty RoleProperty =
            DependencyProperty.Register("Role", typeof(object), typeof(ContentControlCompositionBehavior), new FrameworkPropertyMetadata((sender, e) => ((ContentControlCompositionBehavior)sender)?.Role_Changed()));

        private void Role_Changed()
        {
            VisualComposition.OnTrace(this, "Role changed: " + Role);
            Update();
        }

        /// <summary>
        /// Updates this instance when any of the constraints have changed.
        /// </summary>
        protected override void OnUpdate()
        {
            var regionId = RegionId;
            var role = Role;
            var contentControl = AssociatedObject;

            VisualComposition.OnTrace(this, $"Update {GetType()}, RegionId={regionId}, Role={role}, ContentControl={contentControl}");

            if (contentControl == null)
                return;

            object? exportedItem = null;

            if (!string.IsNullOrEmpty(regionId))
            {
                var exports = GetExports(regionId);
                if (exports == null)
                {
                    VisualComposition.OnTrace(this, $"Update {GetType()}: No exports for RegionId={regionId} found");
                    return;
                }

                exportedItem = exports
                    .Where(item => DataTemplateManager.RoleEquals(item.Metadata?.Role, role))
                    .Select(item => GetTarget(item.Value))
                    .FirstOrDefault();
            }

            VisualComposition.OnTrace(this, $"Update {contentControl.GetType()}, Content={exportedItem?.GetType()}");

            UpdateContent(contentControl, exportedItem);
        }

        private void UpdateContent([NotNull] ContentControl contentControl, [CanBeNull] object? targetItem)
        {
            var currentItem = contentControl.Content;

            if (targetItem != currentItem)
            {
                ApplyContext(currentItem as IComposablePartWithContext, null);
                contentControl.Content = targetItem;
            }

            ApplyContext(targetItem as IComposablePartWithContext, CompositionContext);
        }

        private static void ApplyContext([CanBeNull] IComposablePartWithContext? item, [CanBeNull] object? context)
        {
            if (item == null)
                return;

            item.CompositionContext = context;
        }
    }
}
