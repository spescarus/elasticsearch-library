using System;

namespace SP.ElasticSearchLibrary.Responses
{
    public class Response
    {

        public Response()
        {
            IsOk         = true;
            ErrorMessage = string.Empty;
        }

        public bool IsOk { get; set; }

        public string ErrorMessage { get; set; }

        public Exception Exception { get; set; }
    }
}
