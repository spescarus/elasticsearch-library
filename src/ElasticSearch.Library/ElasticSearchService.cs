using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using SP.ElasticSearchLibrary.Requests;
using SP.ElasticSearchLibrary.Responses;

namespace SP.ElasticSearchLibrary
{
    public sealed class ElasticSearchService : IElasticSearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ElasticSearchSettings _elasticSearchSettings;

        public ElasticSearchService(IElasticClient client,
                                    ElasticSearchSettings elasticSearchSettings)
        {
            _elasticClient = client;
            _elasticSearchSettings = elasticSearchSettings;
            var clusterHealthResponse = _elasticClient.Cluster.Health();
            if (clusterHealthResponse.Status == Health.Red)
            {
                throw new ArgumentException(clusterHealthResponse.DebugInformation);
            }
        }

        public async Task GetAllFields<TEntity>() where TEntity : class

        {
            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var mappingResponse = await _elasticClient.Indices.GetMappingAsync<TEntity>(m => m.Index(indexName));
        }

        public async Task<bool> CheckIfFieldTypeIsTextAsync<TEntity>(string propertyName,
                                                                     string propertyEntityName = null)
            where TEntity : class
        {
            var property = string.IsNullOrWhiteSpace(propertyEntityName)
                               ? propertyName
                               : propertyEntityName;
            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var mappingResponse = await _elasticClient.Indices.GetMappingAsync<TEntity>(m => m.Index(indexName));
            if (!mappingResponse.IsValid)
                return false;

            var propertyDefinition = ElasticSearchExtensions.GetPropertyDefinition(mappingResponse, indexName, property);

            switch (propertyDefinition)
            {
                case NestedProperty nestedProperty:
                    {
                        var propertyProperty = nestedProperty.Properties[propertyName];

                        return string.Equals(propertyProperty.Type, FieldType.Text.ToString(), StringComparison.InvariantCultureIgnoreCase);
                    }
                case null:
                    return false;
                default:
                    {
                        var isText = string.Equals(propertyDefinition.Type, FieldType.Text.ToString(), StringComparison.InvariantCultureIgnoreCase);
                        return isText;
                    }
            }
        }

        private Task<CreateIndexResponse> CreateIndexAsync<TEntity>(string indexName)
            where TEntity : class
        {
            return _elasticClient.Indices
                                 .CreateAsync(indexName, c => c
                                                             .Map<TEntity>(m => m.AutoMap(new AllStringsMultiFieldsVisitor()))
                                                             .Settings(s => s
                                                                           .NumberOfReplicas(_elasticSearchSettings.NumberOfReplicas)
                                                                           .NumberOfShards(_elasticSearchSettings.NumberOfShards)
                                                                           .AutoExpandReplicas("0-all")
                                                                           .Analysis(a => a
                                                                                         .Analyzers(analyzer => analyzer
                                                                                                               .Custom("edge_ngram_analyzer", analyzerDescriptor =>
                                                                                                                           analyzerDescriptor
                                                                                                                              .Tokenizer("edge_ngram")
                                                                                                                              .Filters("lowercase", "asciifolding")
                                                                                                                )
                                                                                                               .Custom("keyword_analyzer", analyzerDescriptor => analyzerDescriptor
                                                                                                                          .Tokenizer("standard")
                                                                                                                          .Filters("lowercase", "asciifolding")
                                                                                                                )
                                                                                                               .Custom("standard", descriptor => descriptor
                                                                                                                          .Tokenizer("standard")
                                                                                                                          .Filters("lowercase", "asciifolding", "stemmer"))
                                                                                          )
                                                                                         .Normalizers(n => n
                                                                                                         .Custom("case_insensitive", nn => nn
                                                                                                                    .Filters("lowercase", "asciifolding")
                                                                                                          )
                                                                                          )
                                                                                         .Tokenizers(tz => tz
                                                                                                        .EdgeNGram("edge_ngram", td => td
                                                                                                                      .TokenChars(TokenChar.Letter,
                                                                                                                               TokenChar.Digit,
                                                                                                                               TokenChar.Punctuation,
                                                                                                                               TokenChar.Symbol
                                                                                                                       )
                                                                                                                      .MinGram(2)
                                                                                                                      .MaxGram(10)
                                                                                                         )
                                                                                          )
                                                                            )
                                                              )
                                  );
        }

        public async Task<Response> RecreateIndexAsync<TEntity>()
            where TEntity : class
        {
            var result = new Response();

            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();
            var existsResponse = await _elasticClient.Indices.ExistsAsync(indexName)
                                                     .ConfigureAwait(false);

            if (existsResponse == null ||
                !existsResponse.Exists)
            {
                return result;
            }

            var deleteResponse = await DeleteIndexAsync(indexName)
                                      .ContinueWith(task =>
                                      {
                                          var response = task.Result;
                                          if (!response.IsOk)
                                          {
                                              result.IsOk = false;
                                              result.ErrorMessage = response.ErrorMessage;
                                              result.Exception = response.Exception;
                                              return result;
                                          }

                                          var createIndexResponse = CreateIndexAsync<TEntity>(indexName);

                                          if (createIndexResponse.Result.IsValid)
                                          {
                                              return result;
                                          }

                                          result.IsOk = createIndexResponse.Result.Acknowledged;
                                          result.ErrorMessage = createIndexResponse.Result.OriginalException.Message;
                                          result.Exception = createIndexResponse.Result.OriginalException;

                                          return result;

                                      })
                                      .ConfigureAwait(false);

            if (deleteResponse.IsOk)
            {
                return result;
            }

            result.IsOk = false;
            result.ErrorMessage = deleteResponse.ErrorMessage;
            result.Exception = deleteResponse.Exception;
            return result;
        }

        public async Task<Response> AddEntityAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            var response = new Response();
            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(indexName)
                                                          .ConfigureAwait(false);

            if (!indexExistsResponse.Exists)
            {
                var indexCreateResponse = await CreateIndexAsync<TEntity>(indexName)
                                             .ConfigureAwait(false);

                if (!indexCreateResponse.IsValid)
                {
                    response.IsOk = false;
                    response.ErrorMessage = indexCreateResponse.OriginalException.Message;
                    response.Exception = indexCreateResponse.OriginalException;
                    return response;
                }
            }

            var entityAddedResult = await _elasticClient.IndexAsync(entity, i => i
                                                                                .Index(indexName)
                                                                                .Id(GetEntityId(entity)))
                                                        .ConfigureAwait(false);

            await _elasticClient.Indices.RefreshAsync(indexName)
                                .ConfigureAwait(false);

            if (entityAddedResult.IsValid) return response;

            response.IsOk = false;
            response.ErrorMessage = entityAddedResult.OriginalException.Message;
            return response;
        }

        public async Task<Response> AddEntitiesAsync<TEntity>(IList<TEntity> entities)
            where TEntity : class
        {
            var response = new Response();

            if (entities == null)
                return response;

            if (!entities.Any())
                return response;

            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(indexName)
                                                          .ConfigureAwait(false);

            if (!indexExistsResponse.Exists)
            {
                var indexCreateResponse = await CreateIndexAsync<TEntity>(indexName)
                                             .ConfigureAwait(false);

                if (!indexCreateResponse.IsValid)
                {
                    response.IsOk = false;
                    response.ErrorMessage = indexCreateResponse.OriginalException.Message;
                    response.Exception = indexCreateResponse.OriginalException;
                    return response;
                }
            }

            var bulkInsert = await _elasticClient.BulkAsync(b => b
                                                               .IndexMany(entities, (op,
                                                                                     item) => op.Index(indexName)
                                                                                                .Id(GetEntityId(item)))
                                                  )
                                                 .ConfigureAwait(false);

            await _elasticClient.Indices.RefreshAsync(indexName)
                                .ConfigureAwait(false);

            if (!bulkInsert.Errors) return response;

            response.IsOk = false;
            response.ErrorMessage = bulkInsert.OriginalException.Message;
            response.Exception = bulkInsert.OriginalException;

            return response;
        }

        public async Task<BulkResponse> AddOrUpdateEntitiesAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            await CreateIndexIfNotExistsAsync<TEntity>();

            var response = await _elasticClient.BulkAsync(b => b.UpdateMany(entities, (op,
                                                                                       item) => op.Index(indexName)
                                                                                                  .DocAsUpsert()
                                                                                                  .Id(new Id(GetEntityId(item)))
                                                                                                  .Doc(item)));
            await _elasticClient.Indices.RefreshAsync(indexName)
                                .ConfigureAwait(false);
            return response;
        }

        public async Task<Response> UpdateEntityAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            var result = new Response();

            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(indexName)
                                                          .ConfigureAwait(false);

            if (!indexExistsResponse.Exists)
            {
                result.IsOk = false;
                result.ErrorMessage = $"Index {indexName} does not exist";
                return result;
            }

            var documentPath = DocumentPath<TEntity>.Id(GetEntityId(entity));

            var updateResponse = await _elasticClient.UpdateAsync(documentPath, u => u
                                                                                    .RetryOnConflict(3)
                                                                                    .Index(indexName)
                                                                                    .DocAsUpsert()
                                                                                    .Doc(entity))
                                                     .ConfigureAwait(false);

            if (updateResponse.IsValid) return result;

            result.IsOk = false;
            result.ErrorMessage = updateResponse.OriginalException.Message;
            result.Exception = updateResponse.OriginalException;
            return result;

        }

        public Task<Response> DeleteEntityAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            var documentPath = DocumentPath<TEntity>.Id(GetEntityId(entity));

            return DeleteEntityFromPathAsync(documentPath);
        }

        public async Task<BulkResponse> DeleteEntitiesAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();
            await CreateIndexIfNotExistsAsync<TEntity>();

            var response = await _elasticClient.BulkAsync(descriptor => descriptor.DeleteMany(entities, (op,
                                                                                                         item) => op.Index(indexName)));
            await _elasticClient.Indices.RefreshAsync(indexName);
            return response;
        }

        public Task<Response> DeleteEntityAsync<TEntity>(Id id)
            where TEntity : class
        {
            var documentPath = DocumentPath<TEntity>.Id(id);

            return DeleteEntityFromPathAsync(documentPath);
        }

        private async Task<Response> DeleteEntityFromPathAsync<TEntity>(DocumentPath<TEntity> documentPath)
            where TEntity : class
        {
            var result = new Response();
            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var existsResponse = await _elasticClient.Indices.ExistsAsync(indexName)
                                                     .ConfigureAwait(false);

            if (!existsResponse.Exists)
            {
                throw new InvalidOperationException($"Index {indexName} does not exist");
            }

            var documentExistsResponse = await _elasticClient.DocumentExistsAsync(documentPath, d => d.Index(indexName))
                                                             .ConfigureAwait(false);

            if (!documentExistsResponse.Exists)
            {
                return result;
            }

            var deleteResponse = await _elasticClient.DeleteAsync(documentPath, d => d.Index(indexName))
                                                     .ConfigureAwait(false);

            if (deleteResponse.IsValid) return result;

            result.IsOk = false;
            result.ErrorMessage = deleteResponse.OriginalException.Message;
            result.Exception = deleteResponse.OriginalException;
            return result;
        }

        public async Task<DocumentResponse<TEntity>> GetEntityByIdAsync<TEntity>(Id id)
            where TEntity : class
        {
            var result = new DocumentResponse<TEntity>();

            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(indexName)
                                                          .ConfigureAwait(false);

            if (!indexExistsResponse.Exists)
            {
                result.IsOk = false;
                result.ErrorMessage = $"Index {indexName} does not exist";

                return result;
            }

            var documentPath = DocumentPath<TEntity>.Id(id);
            var getResponse = await _elasticClient.GetAsync(documentPath, descriptor => descriptor.Index(indexName))
                                                  .ConfigureAwait(false);

            if (!getResponse.IsValid)
            {
                result.IsOk = false;
                result.ErrorMessage = getResponse.OriginalException.Message;
                result.Exception = getResponse.OriginalException;
                return result;

            }

            result.DocumentResult = getResponse;

            return result;
        }

        public async Task<ElasticSearchResponse<TEntity>> SearchAsync<TEntity>(ElasticSearchRequest<TEntity> elasticSearchRequest)
            where TEntity : class
        {
            var result = new ElasticSearchResponse<TEntity>();

            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();

            var searchResponse = await _elasticClient.SearchAsync<TEntity>(s => CreateSearchDescriptor(elasticSearchRequest, s, indexName))
                                                     .ConfigureAwait(false);

            if (!searchResponse.IsValid)
            {
                result.IsOk = false;
                result.ErrorMessage = searchResponse.OriginalException?.Message;
                result.Exception = searchResponse.OriginalException;
                return result;
            }

            result.SearchResults = searchResponse;

            return result;
        }

        public async Task PingAsync()
        {
            var result = await _elasticClient.PingAsync();
            if (!result.IsValid)
            {
                if (result.ServerError == null)
                    throw new Exception($"Elastic Search cluster unreachable");
            }
        }

        public async Task<ClusterHealthResponse> GetClusterHealthAsync()
        {
            var response = await _elasticClient.Cluster.HealthAsync()
                                               .ConfigureAwait(false);
            return response;
        }

        private static ISearchRequest CreateSearchDescriptor<TEntity>(ElasticSearchRequest<TEntity>                         elasticSearchRequest,
                                                                      SearchDescriptor<TEntity>                             descriptor,
                                                                      string                                                indexName)
            where TEntity : class
        {

            var sort = elasticSearchRequest.Sort;

            var searchDescriptor = descriptor
                                  .Index(indexName)
                                  .Query(elasticSearchRequest.Query)
                                  .Sort(sort)
                                  .From(elasticSearchRequest.Offset)
                                  .Take(elasticSearchRequest.Limit);

            if (elasticSearchRequest.Aggregation != null)
            {
                searchDescriptor.Aggregations(elasticSearchRequest.Aggregation);
            }

            return searchDescriptor;
        }

        private async Task<Response> DeleteIndexAsync(string indexName)
        {
            var result = new Response();
            var deleteResponse = await _elasticClient.Indices.DeleteAsync(indexName)
                                                     .ConfigureAwait(false);

            if (deleteResponse.IsValid) return result;

            result.IsOk = false;
            result.ErrorMessage = deleteResponse.OriginalException.Message;
            result.Exception = deleteResponse.OriginalException;
            return result;
        }

        private static string GetEntityId<TEntity>(TEntity entity)
            where TEntity : class
        {
            var idPropertyInfo = entity.GetType()
                                       .GetProperty("Id");

            if (idPropertyInfo != null)
            {
                return idPropertyInfo.GetValue(entity)
                                     .ToString();
            }

            throw new Exception($"Property Id was not found for entity {entity.GetType().Name}");
        }

        private async Task CreateIndexIfNotExistsAsync<TEntity>()
            where TEntity : class
        {
            var indexName = ElasticSearchExtensions.GetIndexNameFrom<TEntity>();
            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(indexName)
                                                          .ConfigureAwait(false);

            if (!indexExistsResponse.Exists)
            {
                await CreateIndexAsync<TEntity>(indexName)
                   .ConfigureAwait(false);
            }
        }
    }
}
