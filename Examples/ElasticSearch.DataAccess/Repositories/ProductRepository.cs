using ElasticSearch.DataAccess.Context;
using ElasticSearch.DataAccess.Repositories.Base;
using ElasticSearch.Models.DbModels;

namespace ElasticSearch.DataAccess.Repositories
{
    public class ProductRepository : Repository<ProductDbModel>, IProductRepository
    {
        public ProductRepository(AppDbContext context)
            : base(context)
        {
        }
    }

    public interface IProductRepository : IRepository<ProductDbModel>
    {
    }
}
