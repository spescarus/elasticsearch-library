using System;

namespace SP.ElasticSearchLibrary.Search.Args.FilterValues
{
    public sealed class DateFilterValue : FilterValue
    {
        public DateTime Value { get; set; }
    }
}