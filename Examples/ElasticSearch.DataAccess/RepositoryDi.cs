using ElasticSearch.DataAccess.Context;
using ElasticSearch.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElasticSearch.DataAccess
{
    public static class RepositoryDi
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(
                (provider, options) =>
                {
                    options.UseSqlServer($"{configuration["ConnectionStrings:DbConnectionString"]}")
                           .EnableDetailedErrors();

                    var loggerFactory = provider.GetService<ILoggerFactory>();
                    if (loggerFactory != null)
                        options.UseLoggerFactory(loggerFactory);
                }, ServiceLifetime.Transient
            );

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
        }
    }
}
