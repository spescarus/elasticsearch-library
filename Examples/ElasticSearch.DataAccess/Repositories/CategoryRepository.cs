using ElasticSearch.DataAccess.Context;
using ElasticSearch.DataAccess.Repositories.Base;
using ElasticSearch.Models.DbModels;

namespace ElasticSearch.DataAccess.Repositories
{
    public class CategoryRepository : Repository<CategoryDbModel>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}
