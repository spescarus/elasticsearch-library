using System.Collections.Generic;

namespace SP.ElasticSearchLibrary.Search.Args
{
    public interface ISearchArgs
    {
        int?                      Offset              { get; set; }
        int?                      Limit               { get; set; }
        string                    SearchText          { get; set; }
        IList<FilterCriteriaArgs> FiltersCriteria     { get; set; }
        IList<SortOptionArgs>     SortOptions         { get; set; }
    }
}
