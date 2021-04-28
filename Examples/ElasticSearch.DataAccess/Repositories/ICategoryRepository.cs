using ElasticSearch.DataAccess.Repositories.Base;
using ElasticSearch.Models.DbModels;

namespace ElasticSearch.DataAccess.Repositories
{
    public interface ICategoryRepository : IRepository<CategoryDbModel>
    {
    }
}
