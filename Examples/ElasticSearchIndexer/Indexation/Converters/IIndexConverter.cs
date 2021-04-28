namespace ElasticSearch.Indexer.Indexation.Converters
{
    public interface IIndexConverter<out TIndex, in TEntity>
    {
        TIndex Convert(TEntity entity);
    }
}
