using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    [ExcludeFromCodeCoverage]
    public class TrackedCollectionTask<TEntity>
        : BaseTrackedTask<TEntity, TEntity[]>,
          ITrackedCollectionTask<TEntity>
        where TEntity : class
    {
        public TrackedCollectionTask(IQueryable<TEntity> query)
            : base(query)
        {
        }

        public override Task<TEntity[]> AsNoTracking()
        {
            return Query.AsNoTracking()
                        .ToArrayAsync();
        }

        public override TaskAwaiter<TEntity[]> GetAwaiter()
        {
            return Query.ToArrayAsync()
                        .GetAwaiter();
        }

        public IPaginatedTrackedCollectionTask<TEntity> Paginate(int offset,
                                                                 int limit)
        {
            return new TrackedPartialCollectionTask<TEntity>(Query, offset, limit);
        }

        public ITrackedCollectionTask<TProjection> Projection<TProjection>(Expression<Func<TEntity, TProjection>> projectionExpression)
            where TProjection : class
        {
            return new TrackedCollectionTask<TProjection>(Query.Select(projectionExpression));
        }
    }
}
