using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    [ExcludeFromCodeCoverage]
    public static class TrackedTaskExtensions
    {
        public static ITrackedTaskSource<TEntity> ToTask<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            return new TrackedTaskSource<TEntity>(query);
        }

        public static async Task<IPartialCollection<TEntity>> ToPartialCollection<TEntity>(this Task<TEntity[]> entityTask)
        {
            var entities = await entityTask;
            return new PartialCollection<TEntity>(entities, entities.Length);
        }

        public static async Task<IPartialCollection<TEntity>> ToPartialTrackedCollection<TEntity>(this ITrackedCollectionTask<TEntity> entityTask)
        {
            var entities = await entityTask;
            return new PartialCollection<TEntity>(entities, entities.Length);
        }
    }
}
