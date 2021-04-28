using ElasticSearch.Indexer.Indexation.Converters;
using ElasticSearch.Indexer.Indexation.Indexers;
using ElasticSearch.Indexer.Indexation.Indexers.Base;
using ElasticSearch.Models.DbModels;
using ElasticSearch.Models.EsModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElasticSearch.Indexer.Indexation
{
    public static class ServiceConfigurations
    {
        public static void ConfigureServices(IServiceCollection services,
                                             IConfiguration     configuration)
        {
            services.AddSingleton<IIndexationProvider, IndexationProvider>();
            services.AddSingleton<IPagedIndexerConfiguration>(new PagedIndexerConfiguration { PageSize = int.Parse(configuration.GetSection("ElasticSearch:PageSize").Value ?? "1000") });

            services.AddTransient<IIndexConverter<ProductIndexItem, ProductDbModel>, ProductIndexItemConverter>();
            services.AddTransient<IIndexer<ProductIndexItem, ProductDbModel>, ProductIndexItemIndexer>();

            services.AddTransient<IIndexConverter<SupplierIndexItem, SupplierDbModel>, SupplierIndexConverter>();
            services.AddTransient<IIndexer<SupplierIndexItem, SupplierDbModel>, SupplierIndexItemIndexer>();

            services.AddTransient<IIndexConverter<CategoryIndexItem, CategoryDbModel>, CategoryIndexConverter>();
            services.AddTransient<IIndexer<CategoryIndexItem, CategoryDbModel>, CategoryIndexItemIndexer>();
        }
    }
}
