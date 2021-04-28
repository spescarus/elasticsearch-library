using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticSearch.Demo.UI.DataTables;
using ElasticSearch.Demo.UI.Services.Products;
using ElasticSearch.Demo.UI.Services.Products.Responses;
using Microsoft.AspNetCore.Mvc;
using SP.ElasticSearchLibrary.Search.Args;
using SP.ElasticSearchLibrary.Search.Args.Enums;

namespace ElasticSearch.Demo.UI.Products
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductSearchService _productSearchService;

        public ProductController(IProductSearchService productSearchService)
        {
            _productSearchService = productSearchService;
        }

        [HttpPost]
        public async Task<IActionResult> SearchProductsAsync([FromBody]DtParameters dtParameters)
        {
            var searchArgs = new SearchArgs
            {
                Offset           = dtParameters.Start,
                Limit            = dtParameters.Length,
                SearchText       = dtParameters.Search?.Value,
                FiltersCriteria  = dtParameters.FiltersCriteria,
                SortOptions      = new List<SortOptionArgs>()
                {
                    ComposeSort(dtParameters)
                }
            };

            var products   = await _productSearchService.SearchProducts(searchArgs);

            return new JsonResult(new DtResult<SearchableProduct>
            {
                Draw            = dtParameters.Draw,
                RecordsTotal    = products.Count,
                RecordsFiltered = products.Count,
                Data            = products.Values
            });
        }

        public SortOptionArgs ComposeSort(DtParameters dtParameters)
        {
            var sort                 = new SortOptionArgs();

            var translatableProperty = dtParameters.SortColumn.Split('.');
            if (translatableProperty.Length == 2)
            {
                sort.PropertyEntityName = translatableProperty[0];
                sort.PropertyName       = translatableProperty[1];
            }
            else
            {
                sort.PropertyName = dtParameters.SortColumn;
            }

            sort.SortOrder = string.IsNullOrWhiteSpace(dtParameters.SortOrder) || dtParameters.SortOrder == "asc"
                                 ? SortOrder.Ascending
                                 : SortOrder.Descending;

            return sort;
        }
    }
}
