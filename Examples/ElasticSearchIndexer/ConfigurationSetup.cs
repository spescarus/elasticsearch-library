using System.IO;
using ElasticSearch.DataAccess;
using ElasticSearch.Indexer.Indexation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Serilog;
using Serilog.Core;
using SP.ElasticSearchLibrary;

namespace ElasticSearch.Indexer
{
    internal static class ConfigurationSetup
    {
        private static IConfiguration _configuration;
        private static Logger         _logger;

        public static (ILogger Logger, IConfiguration Configuration) ConfigurationEnvironment()
        {
            var builder = new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.Development.json")
                         .AddEnvironmentVariables();

            _configuration = builder.Build();
            _logger = new LoggerConfiguration()
                     .ReadFrom.Configuration(_configuration)
                     .CreateLogger();

            return (_logger, _configuration);
        }

        public static ServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(_logger, true);
            });

            RepositoryDi.ConfigureServices(services, _configuration);
            InitializeElasticSearch(services, _configuration);
            ServiceConfigurations.ConfigureServices(services, _configuration);

            return services;
        }


        private static void InitializeElasticSearch(IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment = null)
        {
            var connectionString       = $"{configuration["ElasticSearch:Connection"]}";
            var numberOfReplicasConfig = $"{configuration["ElasticSearch:NumberOfReplicas"]}";
            var numberOfShardsConfig   = $"{configuration["ElasticSearch:NumberOfShards"]}";

            if (string.IsNullOrWhiteSpace(numberOfReplicasConfig) || int.TryParse(numberOfReplicasConfig, out var numberOfReplicas) == false)
            {
                numberOfReplicas = 5;
            }

            if (string.IsNullOrWhiteSpace(numberOfShardsConfig) || int.TryParse(numberOfShardsConfig, out var numberOfShards)== false)
            {
                numberOfShards = 5;
            }

            var enableDebugMode = environment != null && environment.IsDevelopment();

            services.AddSingleton(new ElasticSearchSettings(numberOfReplicas, numberOfShards));
            services.AddSingleton<IElasticClient>(ElasticSearchSetup.Initialize(connectionString, true));
            services.AddSingleton<IElasticSearchService, ElasticSearchService>();
        }
    }
}
