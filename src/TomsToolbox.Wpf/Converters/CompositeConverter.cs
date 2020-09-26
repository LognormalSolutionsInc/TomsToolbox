﻿namespace TomsToolbox.Wpf.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Markup;

    using JetBrains.Annotations;

    /// <summary>
    /// A converter composed of a chain of converters. The converters are invoked in the oder specified.
    /// </summary>
    [ContentProperty("Converters")]
    [ValueConversion(typeof(object), typeof(object))]
    public class CompositeConverter : IValueConverter
    {
        [NotNull, ItemNotNull]
        private readonly Collection<IValueConverter> _converters = new Collection<IValueConverter>(new List<IValueConverter>());

        /// <summary>
        /// Gets the chain of converters.
        /// </summary>
        [NotNull, ItemNotNull]
        public Collection<IValueConverter> Converters
        {
            get
            {
                return _converters;
            }
        }

        #region IValueConverter Members

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.
        /// </returns>
        [CanBeNull]
        public object? Convert([CanBeNull] object? value, [CanBeNull] Type? targetType, [CanBeNull] object? parameter, [CanBeNull] CultureInfo? culture)
        {
            if (Converters.Count <= 0)
            {
                throw new InvalidOperationException("One or more converters are required.");
            }

            return Converters.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.
        /// </returns>
        [CanBeNull]
        public object? ConvertBack([CanBeNull] object? value, [CanBeNull] Type? targetType, [CanBeNull] object? parameter, [CanBeNull] CultureInfo? culture)
        {
            if (Converters.Count <= 0) 
            {
                throw new InvalidOperationException("One or more converters are required.");
            }

            return Converters.Reverse().Aggregate(value, (current, converter) => converter?.ConvertBack(current, targetType, parameter, culture));
        }

        #endregion
    }
}