using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    [ExcludeFromCodeCoverage]
    public class TrackedPartialCollectionTask<TEntity>
        : BaseTrackedTask<TEntity, IPartialCollection<TEntity>>,
          IPaginatedTrackedCollectionTask<TEntity>
        where TEntity : class
    {
        private readonly int                 _limit;
        private readonly int                 _offset;
        private readonly IQueryable<TEntity> _rootQueryable;

        public TrackedPartialCollectionTask(IQueryable<TEntity> query,
                                            int                 offset,
                                            int                 limit)
            : base(query.Skip(offset)
                        .Take(limit))
        {
            _rootQueryable = query;
            _offset        = offset;
            _limit         = limit;
        }

        public override async Task<IPartialCollection<TEntity>> AsNoTracking()
        {
            var query  = Query.AsNoTracking();
            var values = await query.ToArrayAsync();
            var count  = await _rootQueryable.LongCountAsync();
            return new PartialCollection<TEntity>(values, count, _offset, _limit);
        }

        public override TaskAwaiter<IPartialCollection<TEntity>> GetAwaiter()
        {
            return Task<IPartialCollection<TEntity>>.Factory
                                                    .StartNew(() =>
                                                     {
                                                         var values = Query.ToArray();
                                                         var count  = _rootQueryable.LongCount();
                                                         return new PartialCollection<TEntity>(
                                                             values, count, _offset, _limit);
                                                     })
                                                    .GetAwaiter();
        }
    }
}
