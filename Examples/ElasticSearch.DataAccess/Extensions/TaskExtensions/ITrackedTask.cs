using System;
using System.Linq.Expressions;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    public interface ITrackedTask<TEntity> : IBaseTrackedTask<TEntity>
    {
        ITrackedTask<TProjection> Projection<TProjection>(Expression<Func<TEntity, TProjection>> projectionExpression)
            where TProjection : class;
    }
}
