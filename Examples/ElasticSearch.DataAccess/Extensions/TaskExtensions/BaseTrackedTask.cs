using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseTrackedTask<TEntity> : BaseTrackedTask<TEntity, TEntity>
        where TEntity : class
    {
        protected BaseTrackedTask(IQueryable<TEntity> query)
            : base(query)
        {
        }
    }

    [ExcludeFromCodeCoverage]
    public static class BaseTrackedTask
    {
        public static IBaseTrackedTask<TResult> FromResult<TResult>(TResult result)
            where TResult : class
        {
            return new ResultTrackedTask<TResult>(result);
        }

        public static ITrackedCollectionTask<TEntity> FromResults<TEntity>(IEnumerable<TEntity> results)
        {
            return new ResultTrackedCollectionTask<TEntity>(results);
        }

        private class ResultTrackedTask<T> : IBaseTrackedTask<T>
        {
            private readonly T _result;

            public ResultTrackedTask(T result)
            {
                _result = result;
            }

            public Task<T> AsNoTracking()
            {
                return Task.FromResult(_result);
            }

            public TaskAwaiter<T> GetAwaiter()
            {
                return Task.FromResult(_result)
                           .GetAwaiter();
            }
        }

        private class ResultTrackedCollectionTask<TEntity> : ITrackedCollectionTask<TEntity>
        {
            private readonly IEnumerable<TEntity> _results;

            public ResultTrackedCollectionTask(IEnumerable<TEntity> results)
            {
                _results = results;
            }

            public Task<TEntity[]> AsNoTracking()
            {
                return Task.FromResult(_results.ToArray());
            }

            public TaskAwaiter<TEntity[]> GetAwaiter()
            {
                return Task.FromResult(_results.ToArray())
                           .GetAwaiter();
            }

            public IPaginatedTrackedCollectionTask<TEntity> Paginate(int offset,
                                                                     int limit)
            {
                return new ResultPaginatedTrackedCollectionTask<TEntity>(_results, offset, limit);
            }

            public ITrackedCollectionTask<TProjection> Projection<TProjection>(Expression<Func<TEntity, TProjection>> projectionExpression)
                where TProjection : class
            {
                return new ResultTrackedCollectionTask<TProjection>(_results.AsQueryable()
                                                                            .Select(projectionExpression));
            }
        }

        private class ResultPaginatedTrackedCollectionTask<TEntity> : IPaginatedTrackedCollectionTask<TEntity>
        {
            private readonly int _limit;
            private readonly int _offset;
            private readonly IEnumerable<TEntity> _results;

            public ResultPaginatedTrackedCollectionTask(IEnumerable<TEntity> results,
                                                        int offset,
                                                        int limit)
            {
                _results = results;
                _offset = offset;
                _limit = limit;
            }

            public Task<IPartialCollection<TEntity>> AsNoTracking()
            {
                return Task.FromResult(
                    (IPartialCollection<TEntity>)new PartialCollection<TEntity>(
                        _results.ToArray(), _results.Count(), _offset, _limit));
            }

            public TaskAwaiter<IPartialCollection<TEntity>> GetAwaiter()
            {
                return Task.FromResult(
                                (IPartialCollection<TEntity>)new PartialCollection<TEntity>(
                                    _results.ToArray(), _results.Count(), _offset, _limit))
                           .GetAwaiter();
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public abstract class BaseTrackedTask<TEntity, TResult> : IBaseTrackedTask<TResult>
        where TEntity : class
    {
        protected readonly IQueryable<TEntity> Query;

        protected BaseTrackedTask(IQueryable<TEntity> query)
        {
            Query = query;
        }

        public abstract Task<TResult> AsNoTracking();
        public abstract TaskAwaiter<TResult> GetAwaiter();
    }
}
