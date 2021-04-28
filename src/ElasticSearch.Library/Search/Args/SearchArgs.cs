using System.Collections.Generic;

namespace SP.ElasticSearchLibrary.Search.Args
{
    public class SearchArgs : ISearchArgs
    {
        public int?                      Offset              { get; set; }
        public int?                      Limit               { get; set; }
        public string                    SearchText          { get; set; }
        public IList<FilterCriteriaArgs> FiltersCriteria     { get; set; } = new List<FilterCriteriaArgs>();
        public IList<SortOptionArgs>     SortOptions         { get; set; } = new List<SortOptionArgs>();
    }
}
