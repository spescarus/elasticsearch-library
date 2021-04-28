using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ElasticSearch.DataAccess;
using ElasticSearch.DataAccess.Extensions.TaskExtensions;
using ElasticSearch.DataAccess.Repositories;
using ElasticSearch.Indexer.Indexation.Converters;
using ElasticSearch.Indexer.Indexation.Indexers.Base;
using ElasticSearch.Models.DbModels;
using ElasticSearch.Models.EsModels;
using Microsoft.Extensions.Logging;
using SP.ElasticSearchLibrary;

namespace ElasticSearch.Indexer.Indexation.Indexers
{
    public class ProductIndexItemIndexer : BasePagedIndexer<ProductIndexItem, ProductDbModel>
    {
        private readonly IProductRepository _productRepository;

        public ProductIndexItemIndexer(
            IProductRepository productRepository,
            ILogger<BasePagedIndexer<ProductIndexItem, ProductDbModel>> logger,
                 IElasticSearchService                                       elasticSearchService,
                 IIndexConverter<ProductIndexItem, ProductDbModel>           converter,
                 IPagedIndexerConfiguration                                  configuration)
            : base(logger, elasticSearchService, converter, configuration)
        {
            _productRepository = productRepository;
        }

        public override async Task<IPartialCollection<ProductDbModel>> ListAsync(int offset,
                                                                           int limit)
        {
            var includes = new Expression<Func<ProductDbModel, object>>[]
            {
                p => p.Supplier,
                p => p.Category
            };
            var products = await _productRepository.GetAllAsync(includes)
                                                   .Paginate(offset, limit)
                                                   .AsNoTracking();
            return products;
        }

        public override async Task<long> CountAsync()
        {
            return await _productRepository.CountAsync();
        }
    }
}
