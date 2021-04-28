using System.Threading.Tasks;
using ElasticSearch.Demo.UI.Services.Products;
using ElasticSearch.Demo.UI.Services.Products.Responses;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SP.ElasticSearchLibrary.Search.Args;

namespace ElasticSearch.Demo.UI.Pages.Products
{
    public class ProductsListModel : PageModel
    {
        private readonly IProductSearchService   _productSearchService;

        public SelectList Suppliers { get; set; }
        public SelectList Categories { get; set; }

        public ProductsListModel(IProductSearchService productSearchService)
        {
            _productSearchService = productSearchService;
        }

        public async Task OnGetAsync()
        {
            var suppliers  = await _productSearchService.GetSuppliers(new SearchArgs {Limit  = 100});
            var categories = await _productSearchService.GetCategories(new SearchArgs {Limit = 100});

            Suppliers  = new SelectList(suppliers,  nameof(SearchableSupplier.Id), nameof(SearchableSupplier.CompanyName));
            Categories = new SelectList(categories, nameof(SearchableCategory.Id), nameof(SearchableCategory.CategoryName));
        }
    }
}
