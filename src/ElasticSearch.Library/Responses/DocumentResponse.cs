using Nest;

namespace SP.ElasticSearchLibrary.Responses
{
    public class DocumentResponse<T> : Response where T : class
    {
        public IGetResponse<T> DocumentResult { get; set; }
    }
}
