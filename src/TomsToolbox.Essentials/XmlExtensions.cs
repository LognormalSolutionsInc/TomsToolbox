﻿namespace TomsToolbox.Essentials
{
    using System.Xml.Linq;

    using JetBrains.Annotations;

    /// <summary>
    /// Extension methods for <see cref="System.Xml.Linq"/> objects.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Gets the attribute value of an XML element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The value of the attribute, or the <paramref name="defaultValue" /> if no such attribute exists
        /// </returns>
        [CanBeNull]
        public static string? GetAttribute([NotNull] this XElement element, [NotNull] string name, [CanBeNull] string? defaultValue = null)
        {
            return GetAttribute(element, XName.Get(name), defaultValue);
        }

        /// <summary>
        /// Gets the attribute value of an XML element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The value of the attribute, or the <paramref name="defaultValue"/> if no such attribute exists
        /// </returns>
        [CanBeNull]
        public static string? GetAttribute([NotNull] this XElement element, [NotNull] XName name, [CanBeNull] string? defaultValue = null)
        {
            var attribute = element.Attribute(name);

            return attribute?.Value ?? defaultValue;
        }
    }
}
