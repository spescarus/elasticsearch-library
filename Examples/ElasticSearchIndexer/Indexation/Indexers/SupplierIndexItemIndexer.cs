using System.Threading.Tasks;
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
    public class SupplierIndexItemIndexer : BasePagedIndexer<SupplierIndexItem, SupplierDbModel>
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierIndexItemIndexer(ISupplierRepository                                           supplierRepository,
                                        ILogger<BasePagedIndexer<SupplierIndexItem, SupplierDbModel>> logger,
                                        IElasticSearchService                                         elasticSearchService,
                                        IIndexConverter<SupplierIndexItem, SupplierDbModel>           converter,
                                        IPagedIndexerConfiguration                                    configuration)
            : base(logger, elasticSearchService, converter, configuration)
        {
            _supplierRepository = supplierRepository;
        }

        public override async Task<IPartialCollection<SupplierDbModel>> ListAsync(int offset,
                                                                                  int limit)
        {
            return await _supplierRepository.GetAllAsync()
                                              .Paginate(offset, limit)
                                              .AsNoTracking();
        }

        public override async Task<long> CountAsync()
        {
            return await _supplierRepository.CountAsync();
        }
    }
}
