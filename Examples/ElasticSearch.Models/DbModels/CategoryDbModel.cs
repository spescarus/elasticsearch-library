using System.Collections.Generic;

namespace ElasticSearch.Models.DbModels
{
    public class CategoryDbModel : BaseEntity
    {
        public string                      CategoryName { get; set; }
        public string                      Description  { get; set; }
        public byte[]                      Picture      { get; set; }
        public ICollection<ProductDbModel> Products     { get; set; }
    }
}
