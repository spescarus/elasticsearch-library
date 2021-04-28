using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using ElasticSearch.Indexer.Indexation.Indexers.Base;
using ElasticSearch.Models.DbModels;
using ElasticSearch.Models.EsModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ElasticSearch.Indexer
{
    class Program
    {
        private class Options
        {
            public Options(ICollection<string> types)
            {
                Types = types;
            }

            [Option('t', "types", Required = false, HelpText = "Types to index", Separator = ';')]
            public ICollection<string> Types { get; }

            public bool IndexAll => Types == null || !Types.Any();
        }

        private static readonly Dictionary<string, IndexDescription> Indexes = new()
        {
            {"products", new IndexDescription {Name    = "products", IndexerType  = typeof(IIndexer<ProductIndexItem, ProductDbModel>), Type   = typeof(ProductIndexItem)}},
            {"suppliers", new IndexDescription {Name   = "suppliers", IndexerType = typeof(IIndexer<SupplierIndexItem, SupplierDbModel>), Type = typeof(SupplierIndexItem)}},
            { "categories", new IndexDescription { Name = "categories", IndexerType = typeof(IIndexer<CategoryIndexItem, CategoryDbModel>), Type = typeof(CategoryDbModel) } }
        };

        private static IDictionary<string, Exception> _errors;
        private static ServiceProvider                _provider;
        private static ILogger                        _logger;

        static void Main(string[] args)
        {
            _errors = new Dictionary<string, Exception>();

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(Launch);
        }

        private static void Launch(Options options)
        {
            var result = ConfigurationSetup.ConfigurationEnvironment();
            _logger = result.Logger.ForContext(typeof(Program));

            _logger.Information("--------------- Indexation");
            var services = ConfigurationSetup.ConfigureServices();
            services.AddSingleton(result.Configuration);
            _provider = services.BuildServiceProvider();

            Stopwatch s = Stopwatch.StartNew();
            _logger.Information("--------------- Start indexing");

            var types = options.IndexAll
                            ? Indexes.Keys.ToList()
                            : options.Types.Distinct()
                                     .ToList();

            _logger.Information($"--------------- Types to index{Environment.NewLine}{string.Join(Environment.NewLine, types.OrderBy(t => t))}");

            var tasks = types.Select(IndexAsync)
                             .ToList();
            Task.WhenAll(tasks)
                .GetAwaiter()
                .GetResult();

            _logger.Information($"-------------- Done indexing in: {s.Elapsed}");
            LogErrors();
        }

        private static Task IndexAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(@"Can't be null, empty or whitespace", nameof(key));

            if (!Indexes.ContainsKey(key.ToLowerInvariant()))
                throw new InvalidOperationException($"Index {key} doesn't exist");

            var index = Indexes[key];
            var task  = Task.Run(() => Index(index));
            return task;
        }

        private static void Index(IndexDescription index)
        {
            var indexer = (IIndexer) _provider.GetService(index.IndexerType);
            if (index.Type != null)
            {
                _logger.Information($"-------- Recreate index {index.Name} (Type : {index.Type.Name})...");
                indexer.RecreateIndexAsync()
                       .GetAwaiter()
                       .GetResult();
            }

            try
            {
                if (index.Type == null)
                {
                    throw new NullReferenceException("Index type was not provided.");
                }

                _logger.Information($"-------- Start indexing {index.Name} (Type : {index.Type.Name}...");

                indexer.IndexAsync()
                       .GetAwaiter()
                       .GetResult();

                _logger.Information($"-------- Done indexing {index.Name} (Type : {index.Type.Name})");
            }
            catch (Exception ex)
            {
                _errors.Add($"Error during indexation of {index.Name}", ex);
                _logger.Error(ex, $"-------- An error occured indexing {index.Name}");
            }
        }

        private static void LogErrors()
        {
            if (!_errors.Any())
            {
                return;
            }

            _logger.Error($"-------------- Errors occurred");
            foreach (var (errorMessage, exception) in _errors)
            {
                _logger.Error(exception, errorMessage);
            }
        }

        private class IndexDescription
        {
            public Type   IndexerType { get; set; }
            public string Name        { get; set; }
            public Type   Type        { get; set; }
        }
    }
}
