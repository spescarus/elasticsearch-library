using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticSearch.Demo.UI.Services.Products.Responses;
using SP.ElasticSearchLibrary.Search.Args;

namespace ElasticSearch.Demo.UI.Services.Products
{
    public interface IProductSearchService
    {
        Task<IList<SearchableSupplier>> GetSuppliers(SearchArgs                   searchArgs);
        Task<IList<SearchableCategory>> GetCategories(SearchArgs                  searchArgs);
        Task<PartialCollectionModel<SearchableProduct>> SearchProducts(SearchArgs searchArgs);
    }
}
