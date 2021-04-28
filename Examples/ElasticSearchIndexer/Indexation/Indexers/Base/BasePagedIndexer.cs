using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticSearch.DataAccess.Extensions.TaskExtensions;
using ElasticSearch.Indexer.Indexation.Converters;
using Microsoft.Extensions.Logging;
using SP.ElasticSearchLibrary;

namespace ElasticSearch.Indexer.Indexation.Indexers.Base
{
    public abstract class BasePagedIndexer<TIndex, TEntity> : BaseIndexer<TIndex, TEntity>, IPagedIndexer<TIndex, TEntity>
        where TIndex : class
    {
        private readonly IPagedIndexerConfiguration _configuration;

        protected BasePagedIndexer(ILogger<BasePagedIndexer<TIndex, TEntity>> logger,
                                   IElasticSearchService                      elasticSearchService,
                                   IIndexConverter<TIndex, TEntity>           converter,
                                   IPagedIndexerConfiguration                 configuration)
            : base(logger, elasticSearchService, converter)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (_configuration.PageSize <= 0)
                throw new InvalidOperationException("Configuration page size must be superior to zero");
        }

        public abstract Task<IPartialCollection<TEntity>> ListAsync(int offset,
                                                                    int limit);

        public abstract Task<long> CountAsync();

        protected override async Task<(List<TEntity> Entities, ICollection<TIndex> Indexes)> IndexAsync()
        {
            var entities = new List<TEntity>();
            var indexes  = new List<TIndex>();

            var count  = await CountAsync();
            var page   = 0;
            var offset = 0;

            Logger.LogInformation($"Start indexing {typeof(TEntity).Name} to index {typeof(TIndex).Name}...");
            do
            {
                var pageEntities = await ListAsync(offset, _configuration.PageSize);
                var pageIndexes  = await IndexAsync(pageEntities.Values);

                entities.AddRange(pageEntities.Values);
                indexes.AddRange(pageIndexes);

                offset += _configuration.PageSize;
                Logger.LogInformation($"Index {typeof(TEntity).Name} to index {typeof(TIndex).Name} {(offset > count ? count : offset)} / {count}");
                page++;

            } while (page <= count / _configuration.PageSize);

            Logger.LogInformation($"Done indexing {indexes.Count} {typeof(TEntity).Name} to index {typeof(TIndex).Name}");
            return (entities, indexes);
        }
    }
}
