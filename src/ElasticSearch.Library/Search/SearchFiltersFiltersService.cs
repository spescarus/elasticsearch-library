using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Filters;

namespace SP.ElasticSearchLibrary.Search
{
    public class SearchFiltersFiltersService<TIndexItem> : ISearchFiltersService<TIndexItem>
        where TIndexItem : class
    {
        public IList<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>> CreateFilters(ISearchArgs searchArgs)
        {
            IList<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>> filters = new List<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>>();

            if (searchArgs.FiltersCriteria == null)
            {
                return filters;
            }

            var duplicateFields = searchArgs.FiltersCriteria.GroupBy(field => $"{field.ParentPropertyName}.{field.PropertyName}")
                                            .Where(field => field.Count() > 1)
                                            .SelectMany(field => field)
                                            .ToList();

            if (duplicateFields.Any())
            {
                var filter = ComposeDuplicateFieldFilters(duplicateFields);
                filters.Add(filter);
            }

            foreach (var filterCriterion in searchArgs.FiltersCriteria.Except(duplicateFields))
            {
                var filter = ComposeFilter(filterCriterion);

                filters.Add(filter);
            }

            return filters;
        }

        private static Func<QueryContainerDescriptor<TIndexItem>, QueryContainer> ComposeDuplicateFieldFilters(ICollection<FilterCriteriaArgs> duplicateFilterArgs)
        {
            var filterTypeInstance = ComposeFilterFactory.CreateInstance(duplicateFilterArgs.First()
                                                                                            .FilterValue.Type);
            var filter = filterTypeInstance.ComposeFilter<TIndexItem>(duplicateFilterArgs);

            return filter;
        }

        private static Func<QueryContainerDescriptor<TIndexItem>, QueryContainer> ComposeFilter(FilterCriteriaArgs args)
        {
            var filterTypeInstance = ComposeFilterFactory.CreateInstance(args.FilterValue.Type);
            var filter             = filterTypeInstance.ComposeFilter<TIndexItem>(args);

            return filter;
        }
    }
}
