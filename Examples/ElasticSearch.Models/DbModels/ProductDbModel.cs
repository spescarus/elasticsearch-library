namespace ElasticSearch.Models.DbModels
{
    public class ProductDbModel : BaseEntity
    {
        public string          ProductName     { get; set; }
        public int             SupplierId      { get; set; }
        public SupplierDbModel Supplier        { get; set; }
        public int             CategoryId      { get; set; }
        public CategoryDbModel Category        { get; set; }
        public string          QuantityPerUnit { get; set; }
        public decimal         UnitPrice       { get; set; }
        public short           UnitsInStock    { get; set; }
        public short           UnitsOnOrder    { get; set; }
        public short           ReorderLevel    { get; set; }
        public bool            Discontinued    { get; set; }
    }
}
