using ElasticSearch.Models.DbModels;
using ElasticSearch.Models.EsModels;

namespace ElasticSearch.Indexer.Indexation.Converters
{
    public class ProductIndexItemConverter : BaseIndexConverter<ProductIndexItem, ProductDbModel>
    {
        public override ProductIndexItem Convert(ProductDbModel entity)
        {
            var index = new ProductIndexItem
            {
                Id              = entity.Id,
                ProductName     = entity.ProductName,
                QuantityPerUnit = entity.QuantityPerUnit,
                UnitPrice       = entity.UnitPrice,
                UnitsInStock    = entity.UnitsInStock,
                UnitsOnOrder    = entity.UnitsOnOrder,
                ReorderLevel    = entity.ReorderLevel,
                Discontinued    = entity.Discontinued
            };

            if (entity.Category != null)
            {
                index.Category = new CategoryIndexItem
                {
                    Id           = entity.CategoryId,
                    CategoryName = entity.Category.CategoryName,
                    Description  = entity.Category.Description
                };
            }

            if (entity.Supplier != null)
            {
                index.Supplier = new SupplierIndexItem
                {
                    Id           = entity.SupplierId,
                    CompanyName  = entity.Supplier.CompanyName,
                    ContactName  = entity.Supplier.ContactName,
                    ContactTitle = entity.Supplier.ContactTitle
                };
            }

            return index;
        }
    }
}
