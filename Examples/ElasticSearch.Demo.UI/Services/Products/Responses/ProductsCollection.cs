using System.Collections.Generic;

namespace ElasticSearch.Demo.UI.Services.Products.Responses
{
    public class PartialCollectionModel<TEntity>
    {
        /// <summary>
        ///     The entities with pagination
        /// </summary>
        public IReadOnlyCollection<TEntity> Values { get; set; }

        /// <summary>
        ///     The current offset used for query the values
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        ///     The current limit used for query the values
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        ///     The total of entities without pagination
        /// </summary>
        public long Count { get; set; }
    }
}
