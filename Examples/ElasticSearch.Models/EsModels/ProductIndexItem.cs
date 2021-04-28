using Nest;

namespace ElasticSearch.Models.EsModels
{
    public class ProductIndexItem
    {
        public int     Id              { get; set; }
        public string  ProductName     { get; set; }
        public string  QuantityPerUnit { get; set; }
        public decimal UnitPrice       { get; set; }
        public int  UnitsInStock    { get; set; }
        public int  UnitsOnOrder    { get; set; }
        public int  ReorderLevel    { get; set; }
        public bool    Discontinued    { get; set; }

        [Nested]
        public SupplierIndexItem Supplier { get; set; }

        [Nested]
        public CategoryIndexItem Category { get; set; }
    }
}
