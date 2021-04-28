using ElasticSearch.Demo.UI.Services.Products;
using ElasticSearch.Models.EsModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SP.ElasticSearchLibrary;
using SP.ElasticSearchLibrary.Search;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostEnvironment;

namespace ElasticSearch.Demo.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson(options =>
                     {
                         options.SerializerSettings.ContractResolver = new DefaultContractResolver
                         {
                             NamingStrategy = new CamelCaseNamingStrategy()
                         };
                         options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                         options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                     })
                     ;
            services.AddRazorPages();

            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.AddAutoMapper(typeof(Startup));

            InitializeElasticSearch(services, Configuration);

            services.AddTransient<IProductSearchService, ProductSearchService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
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

            if (string.IsNullOrWhiteSpace(numberOfShardsConfig) || int.TryParse(numberOfShardsConfig, out var numberOfShards))
            {
                numberOfShards = 5;
            }

            var enableDebugMode = environment != null && environment.IsDevelopment();

            services.AddSingleton(new ElasticSearchSettings(numberOfReplicas, numberOfShards));
            services.AddSingleton<IElasticClient>(ElasticSearchSetup.Initialize(connectionString, true));
            services.AddSingleton<IElasticSearchService, ElasticSearchService>();
            services.AddTransient<ISearchFiltersService<ProductIndexItem>, SearchFiltersFiltersService<ProductIndexItem>>();
        }
    }
}
