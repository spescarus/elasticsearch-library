using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearch.DataAccess
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> Includes<TEntity>(this   IQueryable<TEntity>                 dbset,
                                                            params Expression<Func<TEntity, object>>[] includes)
            where TEntity : class
        {
            return includes.Aggregate(dbset, (current,
                                              include) => current.Include(include));
        }
    }
}
