namespace TomsToolbox.Wpf.Composition
{
    using JetBrains.Annotations;

    /// <summary>
    /// Common interface for visual composition behaviors.
    /// </summary>
    public interface IVisualCompositionBehavior
    {
        /// <summary>
        /// Gets or sets the id of the region. The region id is used to select candidates for composition.
        /// </summary>
        [CanBeNull]
        string? RegionId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the composition context.
        /// </summary>
        [CanBeNull]
        object? CompositionContext
        {
            get;
            set;
        }
    }
}