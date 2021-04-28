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
    public class TrackedTask<TEntity> : BaseTrackedTask<TEntity>, ITrackedTask<TEntity>
        where TEntity : class
    {
        public TrackedTask(IQueryable<TEntity> query)
            : base(query)
        {
        }

        public override Task<TEntity> AsNoTracking()
        {
            return Query.AsNoTracking()
                        .FirstOrDefaultAsync();
        }

        public override TaskAwaiter<TEntity> GetAwaiter()
        {
            return Query.FirstOrDefaultAsync()
                        .GetAwaiter();
        }

        public ITrackedTask<TProjection> Projection<TProjection>(Expression<Func<TEntity, TProjection>> projectionExpression)
            where TProjection : class
        {
            return new TrackedTask<TProjection>(Query.Select(projectionExpression));
        }
    }
}
