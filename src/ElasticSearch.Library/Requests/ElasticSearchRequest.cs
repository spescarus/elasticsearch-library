using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nest;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Exceptions;
using SortOrder = SP.ElasticSearchLibrary.Search.Args.Enums.SortOrder;

namespace SP.ElasticSearchLibrary.Requests
{
    public sealed class ElasticSearchRequest<TIndexItem>
        where TIndexItem : class
    {
        private static   ElasticSearchRequest<TIndexItem>                                        _searchRequest;
        private readonly IElasticSearchService                                                   _elasticSearchService;
        public           Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>              Query       { get; private set; }
        public           Func<AggregationContainerDescriptor<TIndexItem>, IAggregationContainer> Aggregation { get; private set; }
        public           Func<SortDescriptor<TIndexItem>, IPromise<IList<ISort>>>                Sort        { get; private set; }
        public           int?                                                                    Offset      { get; private set; } = 0;
        public           int?                                                                    Limit       { get; private set; }

        private ElasticSearchRequest(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public static ElasticSearchRequest<TIndexItem> Init(IElasticSearchService elasticSearchService)
        {
            _searchRequest = new ElasticSearchRequest<TIndexItem>(elasticSearchService);
            return _searchRequest;
        }

        public ElasticSearchRequest<TIndexItem> CreateSearchRequestQuery(ISearchArgs                                                             searchArgs,
                                                                         Collection<TextSearchField<TIndexItem>>                                 fieldsToSearch,
                                                                         ICollection<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>> filters        = null)
        {
            Query  = CreateQuery(searchArgs, fieldsToSearch, filters);
            Offset = searchArgs.Offset;
            Limit  = searchArgs.Limit;

            return this;
        }

        public async Task<ElasticSearchRequest<TIndexItem>> CreateSort(ISearchArgs searchArgs)
        {
            Sort = await CreateSort(searchArgs.SortOptions);

            return this;
        }

        public ElasticSearchRequest<TIndexItem> CreateAggregation(Func<AggregationContainerDescriptor<TIndexItem>, IAggregationContainer> aggregation = null)
        {
            Aggregation = aggregation;
            return this;
        }

        public Task<ElasticSearchRequest<TIndexItem>> BuildAsync()
        {
            return Task.FromResult(_searchRequest);
        }

        private static Func<QueryContainerDescriptor<TIndexItem>, QueryContainer> CreateQuery(ISearchArgs                             searchArgs,
                                                                                              Collection<TextSearchField<TIndexItem>> textSearchFields,
                                                                                              ICollection<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>> filters =
                                                                                                  null)
        {

            var boolQueryDescriptor = new BoolQueryDescriptor<TIndexItem>();

            if (!string.IsNullOrWhiteSpace(searchArgs.SearchText))
            {
                if (textSearchFields == null || !textSearchFields.Any())
                {
                    throw new ElasticSearchException("At least one text field need to be specified for text search");
                }

                var textSearch = WildcardTextSearch(searchArgs.SearchText, textSearchFields);
                boolQueryDescriptor.Must(textSearch);
            }

            filters ??= new Collection<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>>();

            boolQueryDescriptor.Filter(filters);

            QueryContainer Query(QueryContainerDescriptor<TIndexItem> q) => q.Bool(b => boolQueryDescriptor);

            return Query;
        }

        /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when sort order option is not provided.</exception>
        private async Task<Func<SortDescriptor<TIndexItem>, IPromise<IList<ISort>>>> CreateSort(IEnumerable<SortOptionArgs> sortOptions)
        {
            var descriptor = new SortDescriptor<TIndexItem>();

            foreach (var sortOption in sortOptions)
            {
                bool isTextField;
                var  translatableProperty = sortOption.PropertyName.Split('.');
                if (translatableProperty.Length == 2)
                {
                    isTextField = string.IsNullOrWhiteSpace(translatableProperty[0])
                                      ? await _elasticSearchService.CheckIfFieldTypeIsTextAsync<TIndexItem>(translatableProperty[1])
                                      : await _elasticSearchService.CheckIfFieldTypeIsTextAsync<TIndexItem>(translatableProperty[1], translatableProperty[0]);

                }
                else
                {
                    isTextField = string.IsNullOrWhiteSpace(sortOption.ParentPropertyName)
                                      ? await _elasticSearchService.CheckIfFieldTypeIsTextAsync<TIndexItem>(sortOption.PropertyName)
                                      : await _elasticSearchService.CheckIfFieldTypeIsTextAsync<TIndexItem>(sortOption.PropertyName, sortOption.ParentPropertyName);
                }

                if (isTextField)
                {
                    sortOption.PropertyName = string.IsNullOrWhiteSpace(sortOption.ParentPropertyName)
                                                  ? $"{sortOption.PropertyName}.keyword"
                                                  : $"{sortOption.ParentPropertyName}.{sortOption.PropertyName}.keyword";
                }

                switch (sortOption.SortOrder)
                {
                    case SortOrder.Ascending:
                        SortAscending(sortOption, descriptor);

                        break;
                    case SortOrder.Descending:
                        SortDescending(sortOption, descriptor);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sortOption.SortOrder), sortOption.SortOrder, null);
                }
            }

            IPromise<IList<ISort>> Sorting(SortDescriptor<TIndexItem> sortDescriptor) => descriptor;

            return Sorting;
        }

        private static void SortDescending(SortOptionArgs             sortOption,
                                           SortDescriptor<TIndexItem> descriptor)
        {
            if (string.IsNullOrWhiteSpace(sortOption.ParentPropertyName))
            {
                descriptor.Field(f => f
                                     .Field(sortOption.PropertyName)
                                     .Descending()
                );
            }
            else
            {
                descriptor.Field(f => f
                                     .Nested(n => n
                                                .Path(sortOption.ParentPropertyName)
                                      )
                                     .Field(sortOption.PropertyName)
                                     .Descending()
                );
            }
        }

        private static void SortAscending(SortOptionArgs             sortOption,
                                          SortDescriptor<TIndexItem> descriptor)
        {
            if (string.IsNullOrWhiteSpace(sortOption.ParentPropertyName))
            {
                descriptor.Field(f => f
                                     .Field(sortOption.PropertyName)
                                     .Ascending()
                );
            }
            else
            {
                descriptor.Field(f => f
                                     .Nested(n => n
                                                .Path(sortOption.ParentPropertyName)
                                      )
                                     .Field(sortOption.PropertyName)
                                     .Ascending()
                );
            }
        }

        private static Func<QueryContainerDescriptor<TIndexItem>, QueryContainer> WildcardTextSearch(string                                  searchText,
                                                                                                     Collection<TextSearchField<TIndexItem>> textSearchFields = null)
        {
            textSearchFields ??= new Collection<TextSearchField<TIndexItem>>
            {
                new TextSearchField<TIndexItem>
                {
                    Field = search => ElasticSearchExtensions.GetField<TIndexItem>("Name")
                }
            };

            var queries = new Collection<Func<QueryContainerDescriptor<TIndexItem>, QueryContainer>>();

            foreach (var field in textSearchFields)
            {
                Func<QueryContainerDescriptor<TIndexItem>, QueryContainer> stringQuerySelector;

                if (field.Strict)
                {
                    stringQuerySelector = MatchExactValue(field);
                }
                else
                {
                    queries.Add(MatchExactValue(field));

                    stringQuerySelector = q => q.MultiMatch(MultiMatchQuerySelector(field.Field));
                }

                if (field.Path == null)
                {
                    queries.Add(stringQuerySelector);
                }
                else
                {
                    QueryContainer NestedQuery(QueryContainerDescriptor<TIndexItem> q) => q
                       .Nested(n => n
                                   .Path(field.Path)
                                   .Query(stringQuerySelector)
                        );

                    queries.Add(NestedQuery);
                }
            }

            return descriptor => descriptor.Bool(b => b.Should(queries));

            Func<MultiMatchQueryDescriptor<TIndexItem>, IMultiMatchQuery> MultiMatchQuerySelector(Expression<Func<TIndexItem, Field>> field)
            {
                return m => m
                           .Query(searchText)
                           .Boost(0.7)
                           .Fields(f => f
                                       .Field(ff => field)
                                       .Field("*.ngram")
                            )
                           .Lenient()
                           .MinimumShouldMatch(MinimumShouldMatch.Percentage(80))
                           .TieBreaker(0.1)
                           .Type(TextQueryType.BestFields)
                           .Operator(Operator.And);
            }

            Func<QueryContainerDescriptor<TIndexItem>, QueryContainer> MatchExactValue(TextSearchField<TIndexItem> field)
            {
                return q => q.QueryString(q1 => q1
                                               .Query(searchText)
                                               .Boost(1.1)
                                               .Fields(field.Field)
                                               .Analyzer("keyword_analyzer")
                                               .DefaultOperator(Operator.And)
                                               .TieBreaker(0.1)
                                               .QuoteFieldSuffix("")
                                               .Rewrite(MultiTermQueryRewrite.TopTermsBlendedFreqs(5))
                                               .Lenient()
                                               .Escape()
                );
            }
        }
    }

    public static class ElasticSearchRequestExtensions
    {
        public static TR Pipe<T, TR>(this T target, Func<T, TR> func) =>
            func(target);

        public static async Task<TR> PipeAsync<T, TR>(this Task<T> target, Func<T, TR> func) =>
            func(await target);

        public static async Task<TR> PipeAsync<T, TR>(this Task<T> target, Func<T, Task<TR>> func) =>
            await func(await target);
    }
}
