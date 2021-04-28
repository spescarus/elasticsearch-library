namespace ElasticSearch.Indexer.Indexation.Converters
{
    public abstract class BaseIndexConverter<TIndex, TEntity> : IIndexConverter<TIndex, TEntity>
        where TIndex : class
    {
        public abstract TIndex Convert(TEntity entity);
    }
}
