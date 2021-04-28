using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SP.ElasticSearchLibrary.Responses;

namespace SP.ElasticSearchLibrary
{
    public interface IElasticSearchService
    {
        Task<bool> CheckIfFieldTypeIsTextAsync<TEntity>(string propertyName,
                                                        string propertyEntityName = null)
            where TEntity : class;

        Task<DocumentResponse<TEntity>> GetEntityByIdAsync<TEntity>(Id id)
            where TEntity : class;

        Task<Response> AddEntityAsync<TEntity>(TEntity entity)
            where TEntity : class;

        Task<Response> AddEntitiesAsync<TEntity>(IList<TEntity> entities)
            where TEntity : class;

        Task<BulkResponse> AddOrUpdateEntitiesAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class;

        Task<Response> UpdateEntityAsync<TEntity>(TEntity entity)
            where TEntity : class;

        Task<Response> DeleteEntityAsync<TEntity>(TEntity entity)
            where TEntity : class;

        Task<Response> DeleteEntityAsync<TEntity>(Id id)
            where TEntity : class;

        Task<BulkResponse> DeleteEntitiesAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class;

        Task<Response> RecreateIndexAsync<T>()
            where T : class;

        Task<ElasticSearchResponse<TEntity>> SearchAsync<TEntity>(Requests.ElasticSearchRequest<TEntity> elasticSearchRequest)
            where TEntity : class;

        Task PingAsync();
        Task<ClusterHealthResponse> GetClusterHealthAsync();
    }
}