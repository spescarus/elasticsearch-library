using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoMapper;
using ElasticSearch.Demo.UI.Services.Products.Responses;
using ElasticSearch.Models.EsModels;
using SP.ElasticSearchLibrary;
using SP.ElasticSearchLibrary.Requests;
using SP.ElasticSearchLibrary.Search;
using SP.ElasticSearchLibrary.Search.Args;

namespace ElasticSearch.Demo.UI.Services.Products
{
    public class ProductSearchService : IProductSearchService
    {
        private readonly IElasticSearchService            _elasticSearchService;
        private readonly ISearchFiltersService<ProductIndexItem> _searchFiltersService;
        private readonly IMapper                          _mapper;

        public ProductSearchService(IElasticSearchService            elasticSearchService,
                                    ISearchFiltersService<ProductIndexItem> searchFiltersService,
                                    IMapper                          mapper)
        {
            _elasticSearchService = elasticSearchService;
            _searchFiltersService        = searchFiltersService;
            _mapper               = mapper;
        }

        public async Task<IList<SearchableCategory>> GetCategories(SearchArgs searchArgs)
        {
            var fieldsToSearch = new Collection<TextSearchField<CategoryIndexItem>>
            {
                new() {Field = field => field.CategoryName}
            };

            var request = await ElasticSearchRequest<CategoryIndexItem>.Init(_elasticSearchService)
                                                                       .CreateSearchRequestQuery(searchArgs, fieldsToSearch)
                                                                       .CreateSort(searchArgs)
                                                                       .PipeAsync(async searchRequest => await searchRequest.BuildAsync());

            var response  = await _elasticSearchService.SearchAsync(request);
            var categories = response.SearchResults.Documents;

            var dtos = _mapper.Map<IList<SearchableCategory>>(categories);

            return dtos;
        }

        public async Task<IList<SearchableSupplier>> GetSuppliers(SearchArgs searchArgs)
        {
            var fieldsToSearch = new Collection<TextSearchField<SupplierIndexItem>>
            {
                new() {Field = field => field.CompanyName}
            };

            var request = await ElasticSearchRequest<SupplierIndexItem>.Init(_elasticSearchService)
                                                                       .CreateSearchRequestQuery(searchArgs, fieldsToSearch)
                                                                       .CreateSort(searchArgs)
                                                                       .PipeAsync(async searchRequest => await searchRequest.BuildAsync());

            var response  = await _elasticSearchService.SearchAsync(request);
            var suppliers = response.SearchResults.Documents;

            var dtos = _mapper.Map<IList<SearchableSupplier>>(suppliers);

            return dtos;
        }

        public async Task<PartialCollectionModel<SearchableProduct>> SearchProducts(SearchArgs searchArgs)
        {

            var fieldsToSearch = new Collection<TextSearchField<ProductIndexItem>>
            {
                new() {Field = field => field.ProductName},
                new() {Path  = path => path.Supplier, Field = field => field.Supplier.CompanyName},
                new() {Path  = path => path.Category, Field = field => field.Category.CategoryName}
            };

            var filters = _searchFiltersService.CreateFilters(searchArgs);

            var request = await ElasticSearchRequest<ProductIndexItem>.Init(_elasticSearchService)
                                                                      .Pipe(searchRequest => searchRequest.CreateSearchRequestQuery(searchArgs, fieldsToSearch, filters))
                                                                      .CreateSort(searchArgs)
                                                                      .PipeAsync(async searchRequest => await searchRequest.BuildAsync());


                      var response = await _elasticSearchService.SearchAsync(request);

            var products = response.SearchResults.Documents;

            return new PartialCollectionModel<SearchableProduct>
            {
                Values = _mapper.Map<IReadOnlyCollection<SearchableProduct>>(products),
                Count  = response.SearchResults.Total,
                Offset = searchArgs.Offset + 1,
                Limit  = response.SearchResults.Hits.Count
            };
        }
    }
}
