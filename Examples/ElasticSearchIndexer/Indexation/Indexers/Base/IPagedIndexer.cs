using System.Threading.Tasks;

namespace ElasticSearch.Indexer.Indexation.Indexers.Base
{
    public interface IPagedIndexer<TIndex, TEntity> : IIndexer<TIndex, TEntity>
        where TIndex : class
    {
        Task<long> CountAsync();
    }
}
