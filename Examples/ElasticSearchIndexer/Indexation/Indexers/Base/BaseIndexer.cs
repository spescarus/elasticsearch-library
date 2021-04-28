using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticSearch.Indexer.Indexation.Converters;
using Microsoft.Extensions.Logging;
using SP.ElasticSearchLibrary;
using SP.ElasticSearchLibrary.Search.Exceptions;

namespace ElasticSearch.Indexer.Indexation.Indexers.Base
{
    public abstract class BaseIndexer<TIndex, TEntity> : IIndexer<TIndex, TEntity>
        where TIndex : class
    {
        private readonly   IIndexConverter<TIndex, TEntity>      _converter;
        private readonly   IElasticSearchService                 _elasticSearchService;
        protected readonly ILogger<BaseIndexer<TIndex, TEntity>> Logger;

        protected BaseIndexer(ILogger<BaseIndexer<TIndex, TEntity>> logger,
                              IElasticSearchService                 elasticSearchService,
                              IIndexConverter<TIndex, TEntity>      converter)
        {
            _converter            = converter            ?? throw new ArgumentNullException(nameof(converter));
            _elasticSearchService = elasticSearchService ?? throw new ArgumentNullException(nameof(elasticSearchService));
            Logger                = logger               ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ICollection<TIndex>> IndexAsync(IReadOnlyCollection<TEntity> entities)
        {
            var indexes = entities.Select(_converter.Convert)
                                  .ToList();

            var response = await _elasticSearchService.AddEntitiesAsync(indexes);

            if (response.IsOk)
            {
                return indexes;
            }

            Logger.LogError($"An error occured during indexing into {typeof(TIndex).Name}", response.Exception);
            throw new ElasticSearchException($"An error occured during indexing into {typeof(TIndex).Name}", response.Exception);
        }

        protected abstract Task<(List<TEntity> Entities, ICollection<TIndex> Indexes)> IndexAsync();

        public async Task RecreateIndexAsync()
        {
            var response = await _elasticSearchService.RecreateIndexAsync<TIndex>();

            if (!response.IsOk)
            {
                Logger.LogError(response.Exception, $"An error occurred during recreate index {typeof(TIndex).Name}");
                throw new ElasticSearchException($"An error occurred during recreate index {typeof(TIndex).Name}", response.Exception);
            }
        }

        async Task<(ICollection<object> Entities, ICollection<object> Indexes)> IIndexer.IndexAsync()
        {
            var result = await IndexAsync();
            return (result.Entities.Cast<object>()
                          .ToList(), result.Indexes.Cast<object>()
                                           .ToList());
        }

    }
}
