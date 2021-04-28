using System;
using System.Collections.Generic;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;

namespace SP.ElasticSearchLibrary.Search
{
    public interface ISearchFiltersService<TIndexItem>
        where TIndexItem : class
    {
        IList<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>> CreateFilters(ISearchArgs  searchArgs);
    }
}
