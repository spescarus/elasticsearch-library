using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Args.FilterValues;

namespace SP.ElasticSearchLibrary.Search.Filters
{
    internal sealed class ComposeGuidFilter : IFilterType
    {
        public Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(ICollection<FilterCriteriaArgs> filterCriteriaArgs)
            where T : class
        {
            var duplicateFields = filterCriteriaArgs.GroupBy(field => $"{field.ParentPropertyName}.{field.PropertyName}")
                                                    .Where(field => field.Count() > 1)
                                                    .SelectMany(field => field)
                                                    .ToList();

            var firstDuplicateField = duplicateFields.First();
            var fieldName = string.IsNullOrWhiteSpace(firstDuplicateField.ParentPropertyName)
                                ? firstDuplicateField.PropertyName
                                : $"{firstDuplicateField.ParentPropertyName}.{firstDuplicateField.PropertyName}";

            IList<Guid?> values = new List<Guid?>();
            foreach (var duplicateField in duplicateFields)
            {
                if (!(duplicateField.FilterValue is GuidFilterValue guidFilterValue))
                {
                    continue;
                }

                values.Add(guidFilterValue.Value);
            }

            QueryContainer GuidFilter(QueryContainerDescriptor<T> q) => q
               .Nested(n => n.Path(firstDuplicateField.ParentPropertyName)
                             .Query(q1 => q1.Terms(m => m.Field(fieldName)
                                                         .Terms(values))));

            return GuidFilter;
        }

        public Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(FilterCriteriaArgs args)
            where T : class
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> filter = descriptor => descriptor;

            if (!(args.FilterValue is GuidFilterValue guidFilterValue))
            {
                return filter;
            }

            if (!guidFilterValue.Value.HasValue)
            {
                return ComposeDoesNotExistFilter<T>(args.ParentPropertyName, args.PropertyName, guidFilterValue);
            }

            if (string.IsNullOrWhiteSpace(args.ParentPropertyName))
            {
                filter = q => q.Term(m => m.Field(args.PropertyName)
                                           .Value(guidFilterValue.Value)
                                           .Verbatim());
            }
            else
            {
                var fieldName = $"{args.ParentPropertyName}.{args.PropertyName}";

                filter = q => q
                   .Nested(n => n
                               .Path(args.ParentPropertyName)
                               .Query(q1 => q1.Term(m => m
                                                        .Field(fieldName)
                                                        .Value(guidFilterValue.Value)
                                      )
                                )
                    );
            }

            return filter;
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> ComposeDoesNotExistFilter<T>(string          propertyEntityName,
                                                                                               string          propertyName,
                                                                                               GuidFilterValue guidFilterValue)
            where T : class
        {
            if (guidFilterValue.Value.HasValue)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(propertyEntityName))
            {
                return q => q
                   .Bool(b => b
                            .MustNot(q1 => q1
                                        .Exists(e => e.Field(propertyName)
                                         )
                             )
                    );
            }

            var fieldName = $"{propertyEntityName}.{propertyName}";

            return q => q
               .Bool(b => b
                        .MustNot(q1 => q1
                                    .Nested(n => n
                                                .Path(propertyEntityName)
                                                .Query(q2 => q2
                                                          .Exists(e => e
                                                                     .Field(fieldName))
                                                 )
                                     )
                         )
                );
        }
    }
}
