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
    public class CategoryIndexItemIndexer : BasePagedIndexer<CategoryIndexItem, CategoryDbModel>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryIndexItemIndexer(ICategoryRepository                                           categoryRepository,
                                        ILogger<BasePagedIndexer<CategoryIndexItem, CategoryDbModel>> logger,
                                        IElasticSearchService                                         elasticSearchService,
                                        IIndexConverter<CategoryIndexItem, CategoryDbModel>           converter,
                                        IPagedIndexerConfiguration                                    configuration)
            : base(logger, elasticSearchService, converter, configuration)
        {
            _categoryRepository = categoryRepository;
        }

        public override async Task<IPartialCollection<CategoryDbModel>> ListAsync(int offset,
                                                                                  int limit)
        {
            return await _categoryRepository.GetAllAsync()
                                              .Paginate(offset, limit)
                                              .AsNoTracking();
        }

        public override async Task<long> CountAsync()
        {
            return await _categoryRepository.CountAsync();
        }
    }
}
