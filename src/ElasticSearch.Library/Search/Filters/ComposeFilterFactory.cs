using System;
using SP.ElasticSearchLibrary.Search.Args.Enums;

namespace SP.ElasticSearchLibrary.Search.Filters
{
    internal sealed class ComposeFilterFactory
    {
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// Thrown when valueType is not part of the enum. Accepted values are Numeric, Date, Text and Guid.
        /// </exception>
        internal static IFilterType CreateInstance(FileterValueType valueType)
        {
            return valueType switch
            {
                FileterValueType.Numeric => new ComposeNumericFilter(),
                FileterValueType.Date    => new ComposeDateFilter(),
                FileterValueType.Text    => new ComposeTextFilter(),
                FileterValueType.Guid    => new ComposeGuidFilter(),
                FileterValueType.Boolean => new ComposeBooleanFilter(),
                _                             => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
            };
        }
    }
}
