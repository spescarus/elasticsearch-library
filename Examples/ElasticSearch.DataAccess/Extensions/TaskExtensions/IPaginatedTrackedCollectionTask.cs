namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    public interface IPaginatedTrackedCollectionTask<TEntity> : IBaseTrackedTask<IPartialCollection<TEntity>>
    {
    }
}
