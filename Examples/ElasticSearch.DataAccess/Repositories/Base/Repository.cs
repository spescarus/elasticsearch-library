using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ElasticSearch.DataAccess.Extensions.TaskExtensions;
using ElasticSearch.Models.DbModels;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearch.DataAccess.Repositories.Base
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected DbContext           Context     { get; }
        protected DbSet<TEntity>      Entities    => Context.Set<TEntity>();
        protected IQueryable<TEntity> EntityQuery => DefaultIncludes(Entities);

        public Repository(DbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual ITrackedCollectionTask<TEntity> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            return EntityQuery
                  .Includes(includes)
                  .ToTask()
                  .ToListAsync();
        }

        public virtual ITrackedCollectionTask<TEntity> GetAllByAsync(Expression<Func<TEntity, bool>> predicate,
                                                                     params Expression<Func<TEntity, object>>[]
                                                                         includes)
        {
            return EntityQuery
                  .Includes(includes)
                  .Where(predicate)
                  .ToTask()
                  .ToListAsync();
        }

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> exp = null)
        {
            var count = exp == null
                            ? await EntityQuery.LongCountAsync()
                            : await EntityQuery.LongCountAsync(exp);

            return count;
        }

        protected virtual IQueryable<TEntity> DefaultIncludes(IQueryable<TEntity> queryable)
        {
            return queryable;
        }
    }
}