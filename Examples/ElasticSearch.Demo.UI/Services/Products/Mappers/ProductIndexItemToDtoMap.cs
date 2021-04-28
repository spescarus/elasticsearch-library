using AutoMapper;
using ElasticSearch.Demo.UI.Services.Products.Responses;
using ElasticSearch.Models.EsModels;

namespace ElasticSearch.Demo.UI.Services.Products.Mappers
{
    public class ProductIndexItemToDtoMap : Profile
    {
        public ProductIndexItemToDtoMap()
        {
            CreateMap<ProductIndexItem, SearchableProduct>()
               .ForMember(p => p.Category, map => map.MapFrom(p => p.Category))
               .ForMember(p => p.Supplier, map => map.MapFrom(p => p.Supplier));

            CreateMap<CategoryIndexItem, SearchableCategory>();
            CreateMap<SupplierIndexItem, SearchableSupplier>();
        }
    }
}
