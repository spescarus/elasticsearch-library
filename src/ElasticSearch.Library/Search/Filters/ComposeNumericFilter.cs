using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Args.Enums;
using SP.ElasticSearchLibrary.Search.Args.FilterValues;

namespace SP.ElasticSearchLibrary.Search.Filters
{
    internal sealed class ComposeNumericFilter : IFilterType
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

            var equalOperator = duplicateFields.Where(field => field.Operator == OperatorType.Equal)
                                               .ToList();
            var differentOperator = duplicateFields.Where(field => field.Operator == OperatorType.Different)
                                                   .ToList();
            var duplicateOperatorFields = duplicateFields
                                         .GroupBy(field => field.Operator)
                                         .Where(field => field.Count() > 1 && field.Any(f => f.Operator != OperatorType.Equal || f.Operator != OperatorType.Different))
                                         .SelectMany(field => field)
                                         .ToList();

            var duplicateOperatorRange = CreateRangeGroupForDuplicateField<T>(duplicateOperatorFields, firstDuplicateField.ParentPropertyName, fieldName);
            var equalOperatorRange     = CreateRangeGroupForDuplicateField<T>(equalOperator,           firstDuplicateField.ParentPropertyName, fieldName);
            var differentOperatorRange = CreateDifferentOperatorFilter<T>(differentOperator, firstDuplicateField.ParentPropertyName, fieldName);

            IList<Func<QueryContainerDescriptor<T>, QueryContainer>> filters = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>
            {
                duplicateOperatorRange,
                equalOperatorRange,
                differentOperatorRange
            };

            var uniqueFilters = duplicateFields.Except(duplicateOperatorFields)
                                               .Except(equalOperator)
                                               .Except(differentOperator)
                                               .ToList();
            if (uniqueFilters.Any())
            {
                var uniqueFilter = CreateRangeForDuplicateField<T>(fieldName, firstDuplicateField.ParentPropertyName, uniqueFilters);
                filters.Add(uniqueFilter);
            }

            QueryContainer Filter(QueryContainerDescriptor<T> q) => q.Bool(b => b.Should(filters));
            return Filter;
        }

        public Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(FilterCriteriaArgs args)
            where T : class
        {
            var fieldName = string.IsNullOrWhiteSpace(args.ParentPropertyName)
                                ? args.PropertyName
                                : $"{args.ParentPropertyName}.{args.PropertyName}";

            if (!(args.FilterValue is NumericFilterValue numericFilterValue))
            {
                return descriptor => descriptor;
            }

            if (args.Operator == OperatorType.Different)
            {
                return DifferentOperatorFilter<T>(fieldName, args.ParentPropertyName, numericFilterValue.Value);
            }

            INumericRangeQuery NumericRange(NumericRangeQueryDescriptor<T> descriptor)
            {
                INumericRangeQuery numericRangeQuery = descriptor
                                                      .Name(fieldName)
                                                      .Field(fieldName)
                                                      .Relation(RangeRelation.Within);

                CreateRangeDescriptor(args.Operator, descriptor, numericFilterValue);

                return numericRangeQuery;
            }

            Func<QueryContainerDescriptor<T>, QueryContainer> filter;
            if (string.IsNullOrWhiteSpace(args.ParentPropertyName))
            {
                filter = q => q.Range(NumericRange);
            }
            else
            {
                filter = q => q.Nested(n => n
                                           .Path(args.ParentPropertyName)
                                           .Query(query => query.Range(NumericRange))
                );
            }

            return filter;
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> CreateRangeForDuplicateField<T>(string                    fieldName,
                                                                                                  string                    propertyEntityName,
                                                                                                  IList<FilterCriteriaArgs> duplicateFields)
            where T : class
        {
            INumericRangeQuery NumericRange(NumericRangeQueryDescriptor<T> descriptor)
            {
                INumericRangeQuery numericRangeQuery = descriptor
                                                      .Name(fieldName)
                                                      .Field(fieldName)
                                                      .Relation(RangeRelation.Within);

                foreach (var duplicateField in duplicateFields)
                {
                    if (!(duplicateField.FilterValue is NumericFilterValue value))
                    {
                        continue;
                    }

                    CreateRangeDescriptor(duplicateField.Operator, descriptor, value);
                }

                return numericRangeQuery;
            }

            Func<QueryContainerDescriptor<T>, QueryContainer> filter;
            if (string.IsNullOrWhiteSpace(propertyEntityName))
            {
                filter = q => q.Range(NumericRange);
            }
            else
            {
                filter = q => q.Nested(n => n
                                           .Path(propertyEntityName)
                                           .Query(query => query.Range(NumericRange))
                );
            }

            return filter;
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> CreateRangeGroupForDuplicateField<T>(IEnumerable<FilterCriteriaArgs> duplicateOperatorFields,
                                                                                                       string                          propertyEntityName,
                                                                                                       string                          fieldName)
            where T : class
        {
            ICollection<Func<QueryContainerDescriptor<T>, QueryContainer>> numericFilterQueries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
            foreach (var duplicateField in duplicateOperatorFields)
            {
                if (!(duplicateField.FilterValue is NumericFilterValue numericFilterValue))
                {
                    continue;
                }

                INumericRangeQuery NumericRange(NumericRangeQueryDescriptor<T> descriptor)
                {
                    INumericRangeQuery numericRangeQuery = descriptor
                                                          .Name($"{fieldName}.{numericFilterValue}")
                                                          .Field(fieldName);

                    CreateRangeDescriptor(duplicateField.Operator, descriptor, numericFilterValue);

                    return numericRangeQuery;
                }

                Func<QueryContainerDescriptor<T>, QueryContainer> filter;
                if (string.IsNullOrWhiteSpace(propertyEntityName))
                {
                    filter = q => q.Range(NumericRange);
                }
                else
                {
                    filter = q => q.Nested(n => n
                                               .Path(propertyEntityName)
                                               .Query(query => query.Range(NumericRange))
                    );
                }

                numericFilterQueries.Add(filter);
            }

            QueryContainer Filter(QueryContainerDescriptor<T> q) => q.Bool(b => b.Should(numericFilterQueries));

            return Filter;
        }

        private void CreateRangeDescriptor<T>(OperatorType                   operatorType,
                                              NumericRangeQueryDescriptor<T> descriptor,
                                              NumericFilterValue             numericFilterValue)
            where T : class
        {
            switch (operatorType)
            {
                case OperatorType.Equal:
                    descriptor.GreaterThanOrEquals(numericFilterValue.Value);
                    descriptor.LessThanOrEquals(numericFilterValue.Value);
                    break;
                case OperatorType.GreaterThan:
                    descriptor.GreaterThan(numericFilterValue.Value);
                    break;
                case OperatorType.GreaterThanOrEqual:
                    descriptor.GreaterThanOrEquals(numericFilterValue.Value);
                    break;
                case OperatorType.LessThan:
                    descriptor.LessThan(numericFilterValue.Value);
                    break;
                case OperatorType.LessThanOrEquals:
                    descriptor.LessThanOrEquals(numericFilterValue.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> CreateDifferentOperatorFilter<T>(IEnumerable<FilterCriteriaArgs> duplicateOperatorFields,
                                                                                                   string                          propertyEntityName,
                                                                                                   string                          fieldName)
            where T : class
        {
            ICollection<Func<QueryContainerDescriptor<T>, QueryContainer>> numericFilterQueries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
            foreach (var duplicateField in duplicateOperatorFields)
            {
                if (!(duplicateField.FilterValue is NumericFilterValue numericFilterValue))
                {
                    continue;
                }

                Func<QueryContainerDescriptor<T>, QueryContainer> filter;

                QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> q) => q
                   .Term(t => t.Field(fieldName)
                               .Value(numericFilterValue.Value));

                if (string.IsNullOrWhiteSpace(propertyEntityName))
                {
                    filter = QueryContainerDescriptor;
                }
                else
                {
                    filter = q => q.Nested(n => n
                                               .Path(propertyEntityName)
                                               .Query(QueryContainerDescriptor)
                    );
                }

                numericFilterQueries.Add(filter);
            }

            QueryContainer Filter(QueryContainerDescriptor<T> q) => q.Bool(b => b.MustNot(numericFilterQueries));

            return Filter;
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> DifferentOperatorFilter<T>(string fieldName,
                                                                                                    string propertyEntityName,
                                                                                                    int?   value)
            where T : class
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> queryContainerDescriptor;

            QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> q) => q
               .Bool(b => b
                        .MustNot(q1 => q1.Term(t => t.Field(fieldName)
                                                     .Value(value))
                         )
                );

            if (string.IsNullOrWhiteSpace(propertyEntityName))
            {
                queryContainerDescriptor = QueryContainerDescriptor;
            }
            else
            {
                queryContainerDescriptor = q => q.Nested(n => n
                                                             .Path(propertyEntityName)
                                                             .Query(QueryContainerDescriptor)
                );
            }

            return queryContainerDescriptor;
        }
    }
}
