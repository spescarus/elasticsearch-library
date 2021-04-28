using Newtonsoft.Json;
using SP.ElasticSearchLibrary.Search.Args.Enums;
using SP.ElasticSearchLibrary.Search.Attributes;

namespace SP.ElasticSearchLibrary.Search.Args.FilterValues
{
    [JsonConverter(typeof(FilterValueJsonConverter))]
    public class FilterValue
    {
        public FileterValueType Type { get; set; }
    }
}