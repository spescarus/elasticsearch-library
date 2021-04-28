using ElasticSearch.Models.DbModels;
using ElasticSearch.Models.EsModels;

namespace ElasticSearch.Indexer.Indexation.Converters
{
    public class CategoryIndexConverter : BaseIndexConverter<CategoryIndexItem, CategoryDbModel>
    {
        public override CategoryIndexItem Convert(CategoryDbModel entity)
        {
            var index = new CategoryIndexItem
            {
                Id           = entity.Id,
                CategoryName = entity.CategoryName,
                Description  = entity.Description
            };

            return index;
        }
    }
}
