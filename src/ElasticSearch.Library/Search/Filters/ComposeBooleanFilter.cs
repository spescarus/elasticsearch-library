using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Args.FilterValues;

namespace SP.ElasticSearchLibrary.Search.Filters
{
    internal sealed class ComposeBooleanFilter : IFilterType
    {
        public Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(ICollection<FilterCriteriaArgs> filterCriteriaArgs)
            where T : class
        {
            var duplicateFields = filterCriteriaArgs.GroupBy(field => $"{field.PropertyEntityName}.{field.PropertyName}")
                                                    .Where(field => field.Count() > 1)
                                                    .SelectMany(field => field)
                                                    .ToList();

            IList<Func<QueryContainerDescriptor<T>, QueryContainer>> queries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
            IList<Func<QueryContainerDescriptor<T>, QueryContainer>> duplicateFilters = duplicateFields.Select(ComposeFilter<T>)
                                                                                                       .ToList();

            QueryContainer GroupDuplicateFieldsFilter(QueryContainerDescriptor<T> q) => q.Bool(b => b.Should(duplicateFilters));

            queries.Add(GroupDuplicateFieldsFilter);

            foreach (var filterCriterion in filterCriteriaArgs.Except(duplicateFields))
            {
                var filter = ComposeFilter<T>(filterCriterion);
                queries.Add(filter);
            }

            QueryContainer Filter(QueryContainerDescriptor<T> q) => q.Bool(b => b.Should(queries));
            return Filter;
        }

        public Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(FilterCriteriaArgs args)
            where T : class
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> filter;

            if (!(args.PropertyFilterValue is BooleanFilterValue value))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(args.PropertyEntityName))
            {
                filter = q => q.Term(t => t.Field(args.PropertyName)
                                           .Value(value.Value));
            }
            else
            {
                var fieldName = $"{args.PropertyEntityName}.{args.PropertyName}";

                filter = q => q
                   .Nested(n => n
                               .Path(args.PropertyEntityName)
                               .Query(q1 => q1
                                         .Term(t => t
                                                   .Field(fieldName)
                                                   .Value(value.Value)
                                          )
                                )
                    );
            }

            return filter;
        }
    }
}
