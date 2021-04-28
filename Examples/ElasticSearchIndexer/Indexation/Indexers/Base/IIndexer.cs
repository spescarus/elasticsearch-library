using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticSearch.Indexer.Indexation.Indexers.Base
{
    public interface IIndexer
    {
        Task<(ICollection<object> Entities, ICollection<object> Indexes)> IndexAsync();

        Task RecreateIndexAsync();

    }

    public interface IIndexer<TIndex, in TEntity> : IIndexer
        where TIndex : class
    {
        Task<ICollection<TIndex>> IndexAsync(IReadOnlyCollection<TEntity> entities);

    }
}
