using System.Globalization;
using Nest;
using SP.ElasticSearchLibrary.Search.Exceptions;

namespace SP.ElasticSearchLibrary
{
    public static class ElasticSearchExtensions
    {
        private static readonly CultureInfo Culture = CultureInfo.CurrentCulture;

        public static string GetIndexNameFrom<TEntity>()
        {
            var typeName = typeof(TEntity).Name.ToLowerInvariant();
            var culture = Culture.ToString()
                                 .ToLowerInvariant();
            return typeName + "_" + culture;
        }

        public static IProperty GetPropertyDefinition(GetMappingResponse mappingResponse,
                                                      string             indexName,
                                                      string             propertyName)
        {
            IProperty propertyDefinition = null;
            if (!mappingResponse.Indices.ContainsKey(indexName))
            {
                return null;
            }

            var mapping = mappingResponse.GetMappingFor(indexName);

            if (mapping.Properties.ContainsKey(propertyName))
            {
                propertyDefinition = mapping.Properties[propertyName];
            }

            return propertyDefinition;
        }

        public static Field GetField<T>(string propertyName)
        {
            var idPropertyInfo = typeof(T).GetProperty(propertyName);

            if (idPropertyInfo != null)
            {
                return new Field(idPropertyInfo);
            }

            var message = $"Property {propertyName} was not found for entity {typeof(T).Name}";

            throw new SearchException(message);
        }
    }
}
