using System;
using System.Linq.Expressions;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    public interface ITrackedCollectionTask<TEntity> : IBaseTrackedTask<TEntity[]>
    {
        IPaginatedTrackedCollectionTask<TEntity> Paginate(int offset,
                                                          int limit);

        ITrackedCollectionTask<TProjection> Projection<TProjection>(Expression<Func<TEntity, TProjection>> projectionExpression)
            where TProjection : class;
    }
}
