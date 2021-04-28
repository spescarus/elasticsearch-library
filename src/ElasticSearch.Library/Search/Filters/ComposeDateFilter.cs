using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Args.Enums;
using SP.ElasticSearchLibrary.Search.Args.FilterValues;

namespace SP.ElasticSearchLibrary.Search.Filters
{
    internal sealed class ComposeDateFilter : IFilterType
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

            var duplicateOperatorRange = CreateRangeGroupForDuplicateField<T>(firstDuplicateField.ParentPropertyName, fieldName, duplicateOperatorFields);
            var equalOperatorRange     = CreateRangeGroupForDuplicateField<T>(firstDuplicateField.ParentPropertyName, fieldName, equalOperator);
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
                var uniqueFilter = CreateRangeForDuplicateField<T>(firstDuplicateField.ParentPropertyName, fieldName, uniqueFilters);
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

            if (!(args.FilterValue is DateFilterValue dateFilterValue))
            {
                return descriptor => descriptor;
            }

            if (args.Operator == OperatorType.Different)
            {
                return DifferentOperatorFilter<T>(fieldName, args.ParentPropertyName, dateFilterValue.Value);
            }

            IDateRangeQuery DateRange(DateRangeQueryDescriptor<T> descriptor)
            {
                IDateRangeQuery dateRangeQuery = descriptor
                                                .Name("")
                                                .Field(fieldName);

                CreateRangeDescriptor(args.Operator, descriptor, dateFilterValue);

                return dateRangeQuery;
            }

            return CreateFilter<T>(args.ParentPropertyName, DateRange);
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> CreateRangeForDuplicateField<T>(string                    propertyEntityName,
                                                                                                  string                    fieldName,
                                                                                                  IList<FilterCriteriaArgs> duplicateFields)
            where T : class
        {
            IDateRangeQuery DateRange(DateRangeQueryDescriptor<T> descriptor)
            {
                IDateRangeQuery dateRangeQuery = descriptor
                                                .Name(fieldName)
                                                .Field(fieldName)
                                                .Relation(RangeRelation.Within);

                foreach (var duplicateField in duplicateFields)
                {
                    if (!(duplicateField.FilterValue is DateFilterValue dateFilterValue))
                    {
                        continue;
                    }

                    CreateRangeDescriptor(duplicateField.Operator, descriptor, dateFilterValue);
                }

                return dateRangeQuery;
            }

            return CreateFilter<T>(propertyEntityName, DateRange);
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> CreateRangeGroupForDuplicateField<T>(string                          propertyEntityName,
                                                                                                       string                          fieldName,
                                                                                                       IEnumerable<FilterCriteriaArgs> duplicateOperatorFields)
            where T : class
        {
            ICollection<Func<QueryContainerDescriptor<T>, QueryContainer>> dateFilterQueries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
            foreach (var duplicateField in duplicateOperatorFields)
            {
                if (!(duplicateField.FilterValue is DateFilterValue dateFilterValue))
                {
                    continue;
                }

                IDateRangeQuery DateRange(DateRangeQueryDescriptor<T> descriptor)
                {
                    IDateRangeQuery numericRangeQuery = descriptor
                                                       .Name($"{fieldName}.{dateFilterValue}")
                                                       .Field(fieldName);

                    CreateRangeDescriptor(duplicateField.Operator, descriptor, dateFilterValue);

                    return numericRangeQuery;
                }

                var dateFilter = CreateFilter<T>(propertyEntityName, DateRange);

                dateFilterQueries.Add(dateFilter);
            }

            QueryContainer Filter(QueryContainerDescriptor<T> q) => q.Bool(b => b.Should(dateFilterQueries));

            return Filter;
        }

        private static void CreateRangeDescriptor<T>(OperatorType                operatorType,
                                                     DateRangeQueryDescriptor<T> descriptor,
                                                     DateFilterValue             dateFilterValue)
            where T : class
        {
            switch (operatorType)
            {
                case OperatorType.Equal:
                    descriptor.GreaterThanOrEquals(dateFilterValue.Value);
                    descriptor.LessThanOrEquals(dateFilterValue.Value.AddHours(24));
                    break;
                case OperatorType.GreaterThan:
                    descriptor.GreaterThan(dateFilterValue.Value);
                    break;
                case OperatorType.GreaterThanOrEqual:
                    descriptor.GreaterThanOrEquals(dateFilterValue.Value);
                    break;
                case OperatorType.LessThan:
                    descriptor.LessThan(dateFilterValue.Value);
                    break;
                case OperatorType.LessThanOrEquals:
                    descriptor.LessThanOrEquals(dateFilterValue.Value.AddHours(24));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> CreateFilter<T>(string                                             propertyEntityName,
                                                                                  Func<DateRangeQueryDescriptor<T>, IDateRangeQuery> dataRangeQueryDescriptor)
            where T : class
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> filter;

            if (string.IsNullOrWhiteSpace(propertyEntityName))
            {
                filter = q => q.DateRange(dataRangeQueryDescriptor);
            }
            else
            {
                filter = q => q
                   .Nested(n => n
                               .Path(propertyEntityName)
                               .Query(a => q
                                         .DateRange(dataRangeQueryDescriptor)
                                )
                    );
            }

            return filter;
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> CreateDifferentOperatorFilter<T>(IEnumerable<FilterCriteriaArgs> duplicateOperatorFields,
                                                                                                          string                          propertyEntityName,
                                                                                                          string                          fieldName)
            where T : class
        {
            ICollection<Func<QueryContainerDescriptor<T>, QueryContainer>> dateFilterQueries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
            foreach (var duplicateField in duplicateOperatorFields)
            {
                if (!(duplicateField.FilterValue is DateFilterValue dateFilterValue))
                {
                    continue;
                }

                Func<QueryContainerDescriptor<T>, QueryContainer> filter;

                QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> q) => q
                   .Term(t => t.Field(fieldName)
                               .Value(dateFilterValue.Value));

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

                dateFilterQueries.Add(filter);
            }

            QueryContainer Filter(QueryContainerDescriptor<T> q) => q.Bool(b => b.MustNot(dateFilterQueries));

            return Filter;
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> DifferentOperatorFilter<T>(string    fieldName,
                                                                                                    string    propertyEntityName,
                                                                                                    DateTime? value)
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
