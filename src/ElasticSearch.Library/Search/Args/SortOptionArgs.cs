using SP.ElasticSearchLibrary.Search.Args.Enums;

namespace SP.ElasticSearchLibrary.Search.Args
{
    public sealed class SortOptionArgs
    {
        public SortOrder SortOrder          { get; set; }
        public string        PropertyName       { get; set; }
        public string        PropertyEntityName { get; set; }
    }
}
