using System;
using ElasticSearch.Indexer.Indexation.Indexers.Base;
using Microsoft.Extensions.DependencyInjection;

namespace ElasticSearch.Indexer.Indexation
{
    public interface IIndexationProvider
    {
        IServiceScope CreateScope();

        IIndexer<TIndex, TEntity> GetInstance<TIndex, TEntity>(IServiceScope scope = null)
            where TIndex : class;

        object GetInstance(Type          type,
                           IServiceScope scope = null);
    }
}
