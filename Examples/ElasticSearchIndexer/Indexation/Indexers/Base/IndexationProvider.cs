using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ElasticSearch.Indexer.Indexation.Indexers.Base
{
    public class IndexationProvider : IIndexationProvider
    {
        private readonly IServiceProvider _provider;

        public IndexationProvider(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IServiceScope CreateScope()
        {
            return _provider.CreateScope();
        }

        public IIndexer<TIndex, TEntity> GetInstance<TIndex, TEntity>(IServiceScope scope = null)
            where TIndex : class
        {
            var provider = scope?.ServiceProvider ?? _provider;
            var instance = provider.GetRequiredService<IIndexer<TIndex, TEntity>>();

            return instance;
        }

        public object GetInstance(Type type, IServiceScope scope = null)
        {
            if (!IsIIndexer(type))
            {
                throw new ArgumentException($@"Type must implement {typeof(IIndexer).Name}", nameof(type));
            }

            var provider = scope?.ServiceProvider ?? _provider;
            var instance = provider.GetRequiredService(type);

            return instance;
        }

        private bool IsIIndexer(Type type)
        {
            var interfaces = type.GetInterfaces();
            var @interface = interfaces.FirstOrDefault(i => i == typeof(IIndexer));

            return @interface != null;
        }
    }
}