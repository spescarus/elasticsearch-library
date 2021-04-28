using ElasticSearch.DataAccess.Context;
using ElasticSearch.DataAccess.Repositories.Base;
using ElasticSearch.Models.DbModels;

namespace ElasticSearch.DataAccess.Repositories
{
    public class SupplierRepository : Repository<SupplierDbModel>, ISupplierRepository
    {
        public SupplierRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}
