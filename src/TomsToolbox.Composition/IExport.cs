﻿namespace TomsToolbox.Composition
{
    using JetBrains.Annotations;

    /// <summary>
    /// Encapsulation of an DI exported object with metadata.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    public interface IExport<out T, out TMetadata>
        where T : class
        where TMetadata: class
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        [CanBeNull]
        T? Value { get; }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        [CanBeNull]
        TMetadata? Metadata { get; }
    }

    /// <summary>
    /// Encapsulation of an DI exported object with generic metadata.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    public interface IExport<out T> : IExport<T, IMetadata>
        where T : class
    {
    }
}