using ElasticSearch.Models.DbModels;
using ElasticSearch.Models.EsModels;

namespace ElasticSearch.Indexer.Indexation.Converters
{
    public class SupplierIndexConverter : BaseIndexConverter<SupplierIndexItem, SupplierDbModel>
    {
        public override SupplierIndexItem Convert(SupplierDbModel entity)
        {
            var index = new SupplierIndexItem
            {
                Id           = entity.Id,
                CompanyName  = entity.CompanyName,
                ContactName  = entity.ContactName,
                ContactTitle = entity.ContactTitle
            };

            return index;
        }
    }
}
