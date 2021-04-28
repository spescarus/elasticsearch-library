using System;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;

namespace SP.ElasticSearchLibrary
{
    public static class ElasticSearchSetup
    {
        public static ElasticClient Initialize(string connectionString,
                                               bool   enableDebugMode)
        {
            var connectionSettings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri(connectionString)),
                                                            (builtin,
                                                             setting) =>
                                                            {
                                                                var jsonNetSerializer = new JsonNetSerializer(
                                                                    builtin, setting, () => new JsonSerializerSettings
                                                                    {
                                                                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                                    });
                                                                return jsonNetSerializer;
                                                            })
               .DisableDirectStreaming(); 

            if (enableDebugMode)
            {
                connectionSettings.EnableDebugMode();
            }

            return new ElasticClient(connectionSettings);
        }
    }
}
