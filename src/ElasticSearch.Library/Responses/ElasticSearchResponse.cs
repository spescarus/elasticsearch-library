using Nest;

namespace SP.ElasticSearchLibrary.Responses
{
    public sealed class ElasticSearchResponse<T> : Response where T : class
    {
        public ISearchResponse<T> SearchResults { get; set; }
    }
}
