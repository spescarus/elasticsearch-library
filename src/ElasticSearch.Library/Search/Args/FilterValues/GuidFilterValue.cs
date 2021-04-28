using System;

namespace SP.ElasticSearchLibrary.Search.Args.FilterValues
{
    public sealed class GuidFilterValue : FilterValue
    {
        public Guid? Value { get; set; }
    }
}