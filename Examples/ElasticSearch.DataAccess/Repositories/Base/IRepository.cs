using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ElasticSearch.DataAccess.Extensions.TaskExtensions;
using ElasticSearch.Models.DbModels;

namespace ElasticSearch.DataAccess.Repositories.Base
{
    public interface IRepository<TEntity>
        where TEntity : BaseEntity
    {
        ITrackedCollectionTask<TEntity> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);

        ITrackedCollectionTask<TEntity> GetAllByAsync([NotNull] Expression<Func<TEntity, bool>>     predicate,
                                                      params    Expression<Func<TEntity, object>>[] includes);

        Task<long> CountAsync(Expression<Func<TEntity, bool>> exp = null);
    }
}
