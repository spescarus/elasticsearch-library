using System;
using System.Collections.Generic;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;

namespace SP.ElasticSearchLibrary.Search.Filters
{
    public interface IFilterType
    {
        Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(ICollection<FilterCriteriaArgs> filterCriteriaArgs)
            where T : class;

        Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(FilterCriteriaArgs args)
            where T : class;

    }
}
