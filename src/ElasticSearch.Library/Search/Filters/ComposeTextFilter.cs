using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Args.Enums;
using SP.ElasticSearchLibrary.Search.Args.FilterValues;

namespace SP.ElasticSearchLibrary.Search.Filters
{
    internal sealed class ComposeTextFilter : IFilterType
    {
        public Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(ICollection<FilterCriteriaArgs> filterCriteriaArgs)
            where T : class
        {
            var duplicateFields = filterCriteriaArgs
                                 .GroupBy(field => $"{field.ParentPropertyName}.{field.PropertyName}")
                                 .Where(field => field.Count() > 1)
                                 .SelectMany(field => field)
                                 .ToList();

            var firstDuplicateField = duplicateFields.First();

            var fieldName = string.IsNullOrWhiteSpace(firstDuplicateField.ParentPropertyName)
                                ? firstDuplicateField.PropertyName
                                : $"{firstDuplicateField.ParentPropertyName}.{firstDuplicateField.PropertyName}";

            var duplicateFieldsEqualOperator = duplicateFields.Where(field => field.Operator == OperatorType.Equal)
                                                              .ToList();
            var duplicateFieldsDifferentOperator = duplicateFields.Where(field => field.Operator == OperatorType.Different)
                                                                  .ToList();

            IList<Func<QueryContainerDescriptor<T>, QueryContainer>> equalOperatorFilters = duplicateFieldsEqualOperator.Select(ComposeFilter<T>)
               .ToList();
            QueryContainer GroupDuplicateFieldsEqualOperatorFilters(QueryContainerDescriptor<T> q) => q.Bool(b => b.Should(equalOperatorFilters));

            var differentOperatorQueries = ComposeDuplicateDifferentOperatorFilter<T>(duplicateFieldsDifferentOperator, firstDuplicateField.ParentPropertyName, fieldName);

            IList<Func<QueryContainerDescriptor<T>, QueryContainer>> queries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>()
            {
                GroupDuplicateFieldsEqualOperatorFilters
            };

            foreach (var filterCriterion in filterCriteriaArgs.Except(duplicateFieldsEqualOperator)
                                                              .Except(duplicateFieldsDifferentOperator))
            {
                var filter = ComposeFilter<T>(filterCriterion);
                queries.Add(filter);
            }

            QueryContainer Filter(QueryContainerDescriptor<T> q) => q.Bool(b => b
                                                                               .Must(queries)
                                                                               .MustNot(differentOperatorQueries)
            );

            return Filter;
        }

        public Func<QueryContainerDescriptor<T>, QueryContainer> ComposeFilter<T>(FilterCriteriaArgs args)
            where T : class
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> filter;

            if (!(args.FilterValue is TextFilterValue value))
            {
                return null;
            }

            if (args.Operator == OperatorType.Different)
            {
                return ComposeDifferentOperatorFilter<T>(args);
            }

            if (string.IsNullOrWhiteSpace(args.ParentPropertyName))
            {
                filter = args.Strict
                             ? q => q.Term(t => t.Field(args.PropertyName)
                                                 .Value(value.Value))
                             : WildcardFilter<T>(value.Value, args.PropertyName, args);
            }
            else
            {
                var fieldName = $"{args.ParentPropertyName}.{args.PropertyName}";

                filter = args.Strict
                             ? (Func<QueryContainerDescriptor<T>, QueryContainer>) (q => q
                                                                                          .Nested(n => n
                                                                                                      .Path(args.ParentPropertyName)
                                                                                                      .Query(q1 => q1
                                                                                                                .Term(t => t
                                                                                                                         .Field(fieldName)
                                                                                                                         .Value(value.Value)
                                                                                                                 )
                                                                                                       )
                                                                                           ))
                             : q => q
                                .Nested(n => n
                                            .Path(args.ParentPropertyName)
                                            .Query(WildcardFilter<T>(value.Value, fieldName, args))
                                 );
            }

            return filter;
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> WildcardFilter<T>(string             searchText,
                                                                                           string             fieldName,
                                                                                           FilterCriteriaArgs args)
            where T : class
        {
            return q => q.MultiMatch(qs => qs.Name($"filter_text_on_{args.PropertyName}")
                                             .Query(searchText)
                                             .Fields(f => f
                                                         .Field(fieldName, boost: 1.1)
                                                         .Field("*.ngram")
                                              )
                                             .Lenient()
                                             .MinimumShouldMatch(MinimumShouldMatch.Percentage(80))
                                             .TieBreaker(0.1)
                                             .Type(TextQueryType.BestFields)
                                             .Operator(Operator.And)
                                             .FuzzyRewrite(MultiTermQueryRewrite.TopTermsBoost(5))
                                             .Fuzziness(Fuzziness.EditDistance(1))
            );
        }

        private static ICollection<Func<QueryContainerDescriptor<T>, QueryContainer>> ComposeDuplicateDifferentOperatorFilter<T>(
            IEnumerable<FilterCriteriaArgs> duplicateOperatorFields,
            string                          propertyEntityName,
            string                          fieldName)
            where T : class
        {
            ICollection<Func<QueryContainerDescriptor<T>, QueryContainer>> textFilterQueries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
            foreach (var duplicateField in duplicateOperatorFields)
            {
                if (!(duplicateField.FilterValue is TextFilterValue textFilterValue))
                {
                    continue;
                }

                Func<QueryContainerDescriptor<T>, QueryContainer> filter;

                QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> q) => q
                   .Term(t => t.Field(fieldName)
                               .Value(textFilterValue.Value));

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

                textFilterQueries.Add(filter);
            }

            return textFilterQueries;
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> ComposeDifferentOperatorFilter<T>(FilterCriteriaArgs args)
            where T : class
        {
            var fieldName = string.IsNullOrWhiteSpace(args.ParentPropertyName)
                                ? args.PropertyName
                                : $"{args.ParentPropertyName}.{args.PropertyName}";

            if (!(args.FilterValue is TextFilterValue textFilterValue))
            {
                return descriptor => descriptor;
            }

            Func<QueryContainerDescriptor<T>, QueryContainer> queryContainerDescriptor;

            QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> q) => q
               .Bool(b => b
                        .MustNot(q1 => q1
                                    .Term(t => t.Field(fieldName)
                                                .Value(textFilterValue.Value))
                         )
                );

            if (string.IsNullOrWhiteSpace(args.ParentPropertyName))
            {
                queryContainerDescriptor = QueryContainerDescriptor;
            }
            else
            {
                queryContainerDescriptor = q => q.Nested(n => n
                                                             .Path(args.ParentPropertyName)
                                                             .Query(QueryContainerDescriptor)
                );
            }

            return queryContainerDescriptor;
        }

    }
}
