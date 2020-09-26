﻿namespace TomsToolbox.Desktop
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using JetBrains.Annotations;

    using TomsToolbox.Essentials;

    /// <summary>
    /// A validation template for using Validar.Fody (<see href="https://github.com/Fody/Validar" />) with data annotations (<see cref="N:System.ComponentModel.DataAnnotations" />).<para />
    /// </summary>
    /// <example>
    /// To activate the validation template just add this line to your AssemblyInfo.cs after you have installed the Validar.Fody package:<para />
    /// <code language="CS">
    /// [assembly: ValidationTemplateAttribute(typeof(TomsToolbox.Desktop.ValidationTemplate))]
    /// </code>
    /// </example>
    public class ValidationTemplate : IDataErrorInfo, INotifyDataErrorInfo
    {
        [NotNull]
        private readonly INotifyPropertyChanged _target;
        [NotNull]
        private readonly ValidationContext _validationContext;
        [NotNull, ItemNotNull]
        private List<ValidationResult> _validationResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationTemplate"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public ValidationTemplate([NotNull] INotifyPropertyChanged target)
        {
            _target = target;
            _validationContext = new ValidationContext(target, null, null);
            _validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(target, _validationContext, _validationResults, true);

            target.PropertyChanged += Validate;
        }

        private void Validate([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            _validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(_target, _validationContext, _validationResults, true);

            _validationResults
                .SelectMany(x => x.MemberNames)
                .Distinct()
                .ForEach(RaiseErrorsChanged);
        }

        /// <inheritdoc />
        [NotNull]
        public string Error
        {
            get
            {
                var strings = _validationResults
                    .Select(x => x.ErrorMessage);

                return string.Join(Environment.NewLine, strings);
            }
        }

        /// <inheritdoc />
        [NotNull]
        public string this[[CanBeNull] string? columnName]
        {
            get
            {
                var strings = _validationResults
                    .Where(x => x.MemberNames.Contains(columnName))
                    .Select(x => x.ErrorMessage);

                return string.Join(Environment.NewLine, strings);
            }
        }

        /// <summary>
        /// Raised when the errors for a property has changed.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void RaiseErrorsChanged([CanBeNull] string? propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        IEnumerable INotifyDataErrorInfo.GetErrors([CanBeNull] string? propertyName)
        {
            return _validationResults
                .Where(x => x.MemberNames.Contains(propertyName))
                .Select(x => x.ErrorMessage);
        }

        bool INotifyDataErrorInfo.HasErrors => _validationResults.Count > 0;
    }
}
