using System.Collections.Generic;

namespace ElasticSearch.Models.DbModels
{
    public class SupplierDbModel : BaseEntity
    {
        public string                      CompanyName  { get; set; }
        public string                      ContactName  { get; set; }
        public string                      ContactTitle { get; set; }
        public string                      Address      { get; set; }
        public string                      City         { get; set; }
        public string                      Region       { get; set; }
        public string                      PostalCode   { get; set; }
        public string                      Country      { get; set; }
        public string                      Phone        { get; set; }
        public string                      Fax          { get; set; }
        public string                      HomePage     { get; set; }
        public ICollection<ProductDbModel> Products      { get; set; }
    }
}
