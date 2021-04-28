using Nest;

namespace ElasticSearch.Models.EsModels
{
    public class CategoryIndexItem
    {
        [Number]

        public int      Id            { get; set; }
        public string CategoryName { get; set; }
        public string Description  { get; set; }
    }

}
