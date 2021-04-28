using System.Collections.Generic;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    public interface IPartialCollection<TEntity>
    {
        IReadOnlyCollection<TEntity> Values { get; }
        long                         Count  { get; }
        int?                         Offset { get; }
        int?                         Limit  { get; }
    }
}
