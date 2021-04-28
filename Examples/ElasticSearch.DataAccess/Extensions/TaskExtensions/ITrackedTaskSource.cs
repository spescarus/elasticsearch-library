namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    public interface ITrackedTaskSource<TEntity>
        where TEntity : class
    {
        ITrackedTask<TEntity> FirstOrDefaultAsync();
        ITrackedCollectionTask<TEntity> ToListAsync();
    }
}
