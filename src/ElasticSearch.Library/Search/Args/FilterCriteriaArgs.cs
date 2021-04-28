using SP.ElasticSearchLibrary.Search.Args.Enums;
using SP.ElasticSearchLibrary.Search.Args.FilterValues;

namespace SP.ElasticSearchLibrary.Search.Args
{
    public sealed class FilterCriteriaArgs
    {
        private string _propertyName;

        public string PropertyName
        {
            get => Strict
                       ? $"{_propertyName}.keyword"
                       : _propertyName;
            set => _propertyName = value;
        }

        public string       ParentPropertyName { get; set; }
        public OperatorType Operator           { get; set; } = OperatorType.Equal;
        public bool         Strict             { get; set; }
        public FilterValue  FilterValue        { get; set; }
    }
}
