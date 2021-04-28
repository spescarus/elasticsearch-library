namespace ElasticSearch.Demo.UI.Services.Products.Responses
{
    public class SearchableProduct
    {
        public int               Id              { get; set; }
        public string            ProductName     { get; set; }
        public string            QuantityPerUnit { get; set; }
        public decimal           UnitPrice       { get; set; }
        public int               UnitsInStock    { get; set; }
        public int               UnitsOnOrder    { get; set; }
        public int               ReorderLevel    { get; set; }
        public bool              Discontinued    { get; set; }
        public SearchableSupplier Supplier        { get; set; }
        public SearchableCategory Category        { get; set; }
    }
}
